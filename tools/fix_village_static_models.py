#!/usr/bin/env python3
"""
Repairs geometric defects in static village GLB models and updates their LOD1/LOD2 counterparts:
1. wieza_straznicza_r0_01.glb: rotates ladder rungs 90 deg so they connect the two ladder rails,
   extends corner legs up to the roof, aligns railings with corner legs, grounds ladder rails.
2. spichlerz_r0_01.glb: extends corner posts up to the roof and adds left/right log walls.
3. stodola_r0_01.glb: fixes overlapping front barn doors and closes wall-to-roof gap.
4. brama_wioskowa_r0_01.glb: swings center gate bars into open side gate wings so the road is open.
5. village_walls/palisade_straight.glb: lowers floating pointed log tips onto the vertical posts.
6. village_walls/palisade_gate.glb: lowers floating pointed log tips and swings gate doors open.
7. ziola_suszone_r0_01.glb: adds left and right vertical support posts under the hanging beam.
8. stos_drewna_r0_02.glb: places front and back vertical retaining stakes on both ends of the woodpile.
"""

import json
import pathlib
import shutil
import struct


def load_glb(path: pathlib.Path):
    data = path.read_bytes()
    magic, version, total_len = struct.unpack_from("<III", data, 0)
    assert magic == 0x46546C67, f"Invalid GLB magic in {path}"
    json_len, json_type = struct.unpack_from("<II", data, 12)
    assert json_type == 0x4E4F534A
    gltf = json.loads(data[20 : 20 + json_len].decode("utf-8"))
    bin_header_offset = 20 + json_len
    bin_len, bin_type = struct.unpack_from("<II", data, bin_header_offset)
    assert bin_type == 0x004E4942
    bin_data = bytearray(data[bin_header_offset + 8 : bin_header_offset + 8 + bin_len])
    return gltf, bin_data


def save_glb(path: pathlib.Path, gltf: dict, bin_data: bytearray):
    while len(bin_data) % 4 != 0:
        bin_data.append(0)
    gltf["buffers"][0]["byteLength"] = len(bin_data)
    json_bytes = json.dumps(gltf, separators=(",", ":")).encode("utf-8")
    while len(json_bytes) % 4 != 0:
        json_bytes += b" "
    total_len = 12 + 8 + len(json_bytes) + 8 + len(bin_data)
    out = bytearray()
    out += struct.pack("<III", 0x46546C67, 2, total_len)
    out += struct.pack("<II", len(json_bytes), 0x4E4F534A)
    out += json_bytes
    out += struct.pack("<II", len(bin_data), 0x004E4942)
    out += bin_data
    path.write_bytes(out)


def get_mesh_positions(gltf: dict, bin_data: bytearray, mesh_index: int):
    prim = gltf["meshes"][mesh_index]["primitives"][0]
    acc_idx = prim["attributes"]["POSITION"]
    acc = gltf["accessors"][acc_idx]
    bv = gltf["bufferViews"][acc["bufferView"]]
    offset = bv.get("byteOffset", 0) + acc.get("byteOffset", 0)
    count = acc["count"]
    verts = [list(struct.unpack_from("<3f", bin_data, offset + i * 12)) for i in range(count)]
    return verts, acc_idx, offset


def set_mesh_positions(gltf: dict, bin_data: bytearray, mesh_index: int, verts: list):
    _, acc_idx, offset = get_mesh_positions(gltf, bin_data, mesh_index)
    for i, v in enumerate(verts):
        struct.pack_into("<3f", bin_data, offset + i * 12, float(v[0]), float(v[1]), float(v[2]))
    acc = gltf["accessors"][acc_idx]
    xs = [float(v[0]) for v in verts]
    ys = [float(v[1]) for v in verts]
    zs = [float(v[2]) for v in verts]
    acc["min"] = [min(xs), min(ys), min(zs)]
    acc["max"] = [max(xs), max(ys), max(zs)]


def remap_axis(val: float, old_min: float, old_max: float, new_min: float, new_max: float) -> float:
    if abs(old_max - old_min) < 1e-6:
        return 0.5 * (new_min + new_max)
    t = (val - old_min) / (old_max - old_min)
    return new_min + t * (new_max - new_min)


def remap_mesh_box(
    gltf: dict,
    bin_data: bytearray,
    mesh_index: int,
    new_x=None,
    new_y=None,
    new_z=None,
):
    verts, _, _ = get_mesh_positions(gltf, bin_data, mesh_index)
    xs = [v[0] for v in verts]
    ys = [v[1] for v in verts]
    zs = [v[2] for v in verts]
    ox0, ox1 = min(xs), max(xs)
    oy0, oy1 = min(ys), max(ys)
    oz0, oz1 = min(zs), max(zs)
    nx0, nx1 = new_x if new_x is not None else (ox0, ox1)
    ny0, ny1 = new_y if new_y is not None else (oy0, oy1)
    nz0, nz1 = new_z if new_z is not None else (oz0, oz1)
    new_verts = [
        [
            remap_axis(v[0], ox0, ox1, nx0, nx1),
            remap_axis(v[1], oy0, oy1, ny0, ny1),
            remap_axis(v[2], oz0, oz1, nz0, nz1),
        ]
        for v in verts
    ]
    set_mesh_positions(gltf, bin_data, mesh_index, new_verts)


def clone_mesh_with_positions(
    gltf: dict,
    bin_data: bytearray,
    template_mesh_index: int,
    new_name: str,
    new_verts: list,
):
    while len(bin_data) % 4 != 0:
        bin_data.append(0)
    byte_offset = len(bin_data)
    for v in new_verts:
        bin_data += struct.pack("<3f", float(v[0]), float(v[1]), float(v[2]))
    byte_length = len(new_verts) * 12

    bv_idx = len(gltf["bufferViews"])
    gltf["bufferViews"].append(
        {
            "buffer": 0,
            "byteOffset": byte_offset,
            "byteLength": byte_length,
            "target": 34962,
        }
    )

    xs = [float(v[0]) for v in new_verts]
    ys = [float(v[1]) for v in new_verts]
    zs = [float(v[2]) for v in new_verts]
    acc_idx = len(gltf["accessors"])
    gltf["accessors"].append(
        {
            "bufferView": bv_idx,
            "byteOffset": 0,
            "componentType": 5126,
            "count": len(new_verts),
            "type": "VEC3",
            "min": [min(xs), min(ys), min(zs)],
            "max": [max(xs), max(ys), max(zs)],
        }
    )

    tmpl_prim = gltf["meshes"][template_mesh_index]["primitives"][0]
    new_attrs = dict(tmpl_prim["attributes"])
    new_attrs["POSITION"] = acc_idx
    new_prim = {
        "attributes": new_attrs,
        "indices": tmpl_prim["indices"],
        "mode": tmpl_prim.get("mode", 4),
    }
    if "material" in tmpl_prim:
        new_prim["material"] = tmpl_prim["material"]

    mesh_idx = len(gltf["meshes"])
    gltf["meshes"].append(
        {
            "name": new_name,
            "primitives": [new_prim],
        }
    )

    node_idx = len(gltf["nodes"])
    gltf["nodes"].append({"name": new_name, "mesh": mesh_idx})
    gltf["nodes"][0].setdefault("children", []).append(node_idx)


def fix_watchtower(root: pathlib.Path):
    path = root / "assets/models/static/wieza_straznicza_r0_01.glb"
    gltf, bin_data = load_glb(path)
    for i, m in enumerate(gltf["meshes"]):
        name = m.get("name", "")
        if name.startswith("leg_"):
            remap_mesh_box(gltf, bin_data, i, new_z=(0.0, 4.95))
        elif name.startswith("railx_-1.5_"):
            remap_mesh_box(gltf, bin_data, i, new_x=(-1.42, 1.42), new_y=(-1.36, -1.24))
        elif name.startswith("railx_1.5_"):
            remap_mesh_box(gltf, bin_data, i, new_x=(-1.42, 1.42), new_y=(1.24, 1.36))
        elif name.startswith("raily_-1.5_"):
            remap_mesh_box(gltf, bin_data, i, new_x=(-1.36, -1.24), new_y=(-1.42, 1.42))
        elif name.startswith("raily_1.5_"):
            remap_mesh_box(gltf, bin_data, i, new_x=(1.24, 1.36), new_y=(-1.42, 1.42))
        elif name.startswith("rung_"):
            # Rotate rung 90 degrees so it spans Y=[-0.39, 0.39] between ladder_-0.35 and ladder_0.35
            remap_mesh_box(gltf, bin_data, i, new_x=(-1.74, -1.66), new_y=(-0.39, 0.39))
        elif name.startswith("ladder_"):
            remap_mesh_box(gltf, bin_data, i, new_z=(0.0, 3.85))

    save_glb(path, gltf, bin_data)
    sync_lod_files(root, path)


def fix_granary(root: pathlib.Path):
    path = root / "assets/models/static/spichlerz_r0_01.glb"
    gltf, bin_data = load_glb(path)
    front_meshes = []
    for i, m in enumerate(gltf["meshes"]):
        name = m.get("name", "")
        if name.startswith("post_"):
            remap_mesh_box(gltf, bin_data, i, new_z=(0.0, 3.00))
        elif name.startswith("front_"):
            front_meshes.append((i, name))

    # Remove previously added left_/right_ if script is re-run
    existing_names = {m.get("name", "") for m in gltf["meshes"]}
    for idx, fname in front_meshes:
        suffix = fname[len("front_") :]
        left_name = f"left_{suffix}"
        right_name = f"right_{suffix}"
        if left_name in existing_names or right_name in existing_names:
            continue
        verts, _, _ = get_mesh_positions(gltf, bin_data, idx)
        # front log is centered at Y = -1.20, spanning X in [-1.70, 1.70].
        # Transform (x, y, z) -> (x_wall + (y + 1.20), -x * (1.25 / 1.70), z) has det > 0!
        scale_y = 1.25 / 1.70
        left_verts = [[-1.60 + (v[1] + 1.20), -v[0] * scale_y, v[2]] for v in verts]
        right_verts = [[1.60 + (v[1] + 1.20), -v[0] * scale_y, v[2]] for v in verts]
        clone_mesh_with_positions(gltf, bin_data, idx, left_name, left_verts)
        clone_mesh_with_positions(gltf, bin_data, idx, right_name, right_verts)

    save_glb(path, gltf, bin_data)
    sync_lod_files(root, path)


def fix_barn(root: pathlib.Path):
    path = root / "assets/models/static/stodola_r0_01.glb"
    gltf, bin_data = load_glb(path)
    for i, m in enumerate(gltf["meshes"]):
        name = m.get("name", "")
        if name == "door_-0.55":
            remap_mesh_box(gltf, bin_data, i, new_x=(-2.62, -0.06), new_z=(0.05, 2.65))
        elif name == "door_0.55":
            remap_mesh_box(gltf, bin_data, i, new_x=(0.06, 2.62), new_z=(0.05, 2.65))
        elif name in ("rear", "side_-2.7", "side_2.7"):
            remap_mesh_box(gltf, bin_data, i, new_z=(0.05, 2.75))

    save_glb(path, gltf, bin_data)
    sync_lod_files(root, path)


def fix_village_gate(root: pathlib.Path):
    path = root / "assets/models/static/brama_wioskowa_r0_01.glb"
    gltf, bin_data = load_glb(path)
    for i, m in enumerate(gltf["meshes"]):
        name = m.get("name", "")
        # Swing the 4 horizontal bars out of the center roadway so they form open double gate leaves
        if name == "gatebar_0.55":
            remap_mesh_box(gltf, bin_data, i, new_x=(-1.46, -1.32), new_y=(-1.35, -0.05), new_z=(0.15, 2.25))
        elif name == "gatebar_1.15":
            remap_mesh_box(gltf, bin_data, i, new_x=(1.32, 1.46), new_y=(-1.35, -0.05), new_z=(0.15, 2.25))
        elif name == "gatebar_1.75":
            remap_mesh_box(gltf, bin_data, i, new_x=(-1.85, 1.85), new_y=(-0.22, -0.04), new_z=(2.62, 2.78))
        elif name == "gatebar_2.35":
            remap_mesh_box(gltf, bin_data, i, new_x=(-1.85, 1.85), new_y=(0.04, 0.22), new_z=(2.62, 2.78))

    save_glb(path, gltf, bin_data)
    sync_lod_files(root, path)


def fix_palisade_straight(root: pathlib.Path):
    path = root / "assets/models/static/village_walls/palisade_straight.glb"
    gltf, bin_data = load_glb(path)
    # Meshes 0..25 are 13 pairs of (vertical log, pointed tip)
    for k in range(13):
        log_idx = 2 * k
        tip_idx = 2 * k + 1
        log_verts, _, _ = get_mesh_positions(gltf, bin_data, log_idx)
        tip_verts, _, _ = get_mesh_positions(gltf, bin_data, tip_idx)
        log_top = max(v[2] for v in log_verts)
        tip_bottom = min(v[2] for v in tip_verts)
        dz = (log_top - 0.01) - tip_bottom
        if abs(dz) > 1e-4:
            new_tip_verts = [[v[0], v[1], v[2] + dz] for v in tip_verts]
            set_mesh_positions(gltf, bin_data, tip_idx, new_tip_verts)

    save_glb(path, gltf, bin_data)
    sync_lod_files(root, path)


def fix_palisade_gate(root: pathlib.Path):
    path = root / "assets/models/static/village_walls/palisade_gate.glb"
    gltf, bin_data = load_glb(path)
    # Meshes 0..23 are 12 pairs of (vertical log, pointed tip)
    for k in range(12):
        log_idx = 2 * k
        tip_idx = 2 * k + 1
        log_verts, _, _ = get_mesh_positions(gltf, bin_data, log_idx)
        tip_verts, _, _ = get_mesh_positions(gltf, bin_data, tip_idx)
        log_top = max(v[2] for v in log_verts)
        tip_bottom = min(v[2] for v in tip_verts)
        dz = (log_top - 0.01) - tip_bottom
        if abs(dz) > 1e-4:
            new_tip_verts = [[v[0], v[1], v[2] + dz] for v in tip_verts]
            set_mesh_positions(gltf, bin_data, tip_idx, new_tip_verts)

    # Swing the two gate doors (25, 27 on left; 26, 28 on right) open 90 degrees outward
    remap_mesh_box(gltf, bin_data, 25, new_x=(-1.24, -1.14), new_y=(-1.22, -0.10), new_z=(0.045, 1.995))
    remap_mesh_box(gltf, bin_data, 26, new_x=(1.14, 1.24), new_y=(-1.22, -0.10), new_z=(0.045, 1.995))
    remap_mesh_box(gltf, bin_data, 27, new_x=(-1.30, -1.21), new_y=(-1.24, -0.08), new_z=(0.175, 1.875))
    remap_mesh_box(gltf, bin_data, 28, new_x=(1.21, 1.30), new_y=(-1.24, -0.08), new_z=(0.175, 1.875))

    save_glb(path, gltf, bin_data)
    sync_lod_files(root, path)


def fix_dried_herbs(root: pathlib.Path):
    path = root / "assets/models/static/ziola_suszone_r0_01.glb"
    gltf, bin_data = load_glb(path)
    existing_names = {m.get("name", "") for m in gltf["meshes"]}
    beam_idx = 0
    beam_verts, _, _ = get_mesh_positions(gltf, bin_data, beam_idx)
    bx0, bx1 = min(v[0] for v in beam_verts), max(v[0] for v in beam_verts)
    by0, by1 = min(v[1] for v in beam_verts), max(v[1] for v in beam_verts)
    bz0, bz1 = min(v[2] for v in beam_verts), max(v[2] for v in beam_verts)

    if "post_-0.8" not in existing_names:
        left_verts = [
            [
                remap_axis(v[0], bx0, bx1, -0.84, -0.74),
                remap_axis(v[1], by0, by1, -0.05, 0.05),
                remap_axis(v[2], bz0, bz1, 0.0, 1.74),
            ]
            for v in beam_verts
        ]
        right_verts = [
            [
                remap_axis(v[0], bx0, bx1, 0.74, 0.84),
                remap_axis(v[1], by0, by1, -0.05, 0.05),
                remap_axis(v[2], bz0, bz1, 0.0, 1.74),
            ]
            for v in beam_verts
        ]
        clone_mesh_with_positions(gltf, bin_data, beam_idx, "post_-0.8", left_verts)
        clone_mesh_with_positions(gltf, bin_data, beam_idx, "post_0.8", right_verts)

    save_glb(path, gltf, bin_data)
    sync_lod_files(root, path)


def fix_woodpile(root: pathlib.Path):
    path = root / "assets/models/static/stos_drewna_r0_02.glb"
    gltf, bin_data = load_glb(path)
    existing_names = {m.get("name", "") for m in gltf["meshes"]}
    s_left_idx = None
    s_right_idx = None
    for i, m in enumerate(gltf["meshes"]):
        name = m.get("name", "")
        if name == "support_-1.0":
            s_left_idx = i
            remap_mesh_box(gltf, bin_data, i, new_x=(-1.05, -0.95), new_y=(-0.62, -0.50), new_z=(0.0, 1.42))
        elif name == "support_1.0":
            s_right_idx = i
            remap_mesh_box(gltf, bin_data, i, new_x=(0.95, 1.05), new_y=(-0.62, -0.50), new_z=(0.0, 1.42))

    if s_left_idx is not None and "support_-1.0_back" not in existing_names:
        v_left, _, _ = get_mesh_positions(gltf, bin_data, s_left_idx)
        v_left_back = [[v[0], -v[1], v[2]] for v in v_left]
        # Flip X order to keep positive determinant when reflecting Y -> -Y
        ox0, ox1 = min(v[0] for v in v_left_back), max(v[0] for v in v_left_back)
        v_left_back = [[ox0 + ox1 - v[0], v[1], v[2]] for v in v_left_back]
        clone_mesh_with_positions(gltf, bin_data, s_left_idx, "support_-1.0_back", v_left_back)

    if s_right_idx is not None and "support_1.0_back" not in existing_names:
        v_right, _, _ = get_mesh_positions(gltf, bin_data, s_right_idx)
        ox0, ox1 = min(v[0] for v in v_right), max(v[0] for v in v_right)
        v_right_back = [[ox0 + ox1 - v[0], -v[1], v[2]] for v in v_right]
        clone_mesh_with_positions(gltf, bin_data, s_right_idx, "support_1.0_back", v_right_back)

    save_glb(path, gltf, bin_data)
    sync_lod_files(root, path)


def sync_lod_files(root: pathlib.Path, src_path: pathlib.Path):
    fname = src_path.name
    for lod in ("lod1", "lod2"):
        dst = root / "assets/lod" / lod / fname
        if dst.parent.exists():
            shutil.copyfile(src_path, dst)


def main():
    root = pathlib.Path(__file__).resolve().parent.parent
    fix_watchtower(root)
    fix_granary(root)
    fix_barn(root)
    fix_village_gate(root)
    fix_palisade_straight(root)
    fix_palisade_gate(root)
    fix_dried_herbs(root)
    fix_woodpile(root)
    print("Successfully repaired all 8 static village GLB models and synced LOD1/LOD2.")


if __name__ == "__main__":
    main()
