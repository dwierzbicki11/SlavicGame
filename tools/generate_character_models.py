#!/usr/bin/env python3
"""Generate sculpted, multi-material, normal-mapped Slavic character and creature GLB models.

Preserves the exact skeleton node hierarchy and keyframe animation channels of all 16
assets/models/animated/*.glb files while replacing placeholder blocky cubes with
anatomically contoured multi-primitive meshes and smooth vertex normals.
"""
import glob
import json
import math
import os
import struct
from pathlib import Path

FLOAT = 5126
UNSIGNED_INT = 5125
ARRAY_BUFFER = 34962
ELEMENT_ARRAY_BUFFER = 34963


def v_add(a, b):
    return (a[0] + b[0], a[1] + b[1], a[2] + b[2])


def v_sub(a, b):
    return (a[0] - b[0], a[1] - b[1], a[2] - b[2])


def v_cross(a, b):
    return (
        a[1] * b[2] - a[2] * b[1],
        a[2] * b[0] - a[0] * b[2],
        a[0] * b[1] - a[1] * b[0],
    )


def v_norm(a):
    l = math.sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2])
    if l < 1e-8:
        return (0.0, 0.0, 1.0)
    return (a[0] / l, a[1] / l, a[2] / l)


class PartBuilder:
    def __init__(self, material_index):
        self.material_index = material_index
        self.positions = []
        self.indices = []

    def add_ring_loft(self, rings, close_bottom=True, close_top=True):
        """Loft a list of rings; each ring is a list of (x, y, z) points in CCW order viewed from +axis."""
        if len(rings) < 2:
            return
        seg = len(rings[0])
        base_idx = len(self.positions)
        for r in rings:
            self.positions.extend(r)

        for r_i in range(len(rings) - 1):
            r0 = base_idx + r_i * seg
            r1 = base_idx + (r_i + 1) * seg
            for s in range(seg):
                sn = (s + 1) % seg
                a = r0 + s
                b = r0 + sn
                c = r1 + sn
                d = r1 + s
                self.indices.extend([a, b, c, a, c, d])

        if close_bottom:
            first_ring = rings[0]
            cx = sum(p[0] for p in first_ring) / seg
            cy = sum(p[1] for p in first_ring) / seg
            cz = sum(p[2] for p in first_ring) / seg
            c_idx = len(self.positions)
            self.positions.append((cx, cy, cz))
            for s in range(seg):
                sn = (s + 1) % seg
                self.indices.extend([c_idx, base_idx + sn, base_idx + s])

        if close_top:
            last_ring = rings[-1]
            cx = sum(p[0] for p in last_ring) / seg
            cy = sum(p[1] for p in last_ring) / seg
            cz = sum(p[2] for p in last_ring) / seg
            c_idx = len(self.positions)
            self.positions.append((cx, cy, cz))
            top_start = base_idx + (len(rings) - 1) * seg
            for s in range(seg):
                sn = (s + 1) % seg
                self.indices.extend([c_idx, top_start + s, top_start + sn])

    def add_z_loft(self, slices, seg=12, close_bottom=True, close_top=True, pleat_freq=0, pleat_amp=0.0):
        """slices: list of (z, rx, ry, cx, cy, front_bulge=0.0)"""
        rings = []
        for s_idx, s_item in enumerate(slices):
            z, rx, ry, cx, cy = s_item[:5]
            f_bulge = s_item[5] if len(s_item) > 5 else 0.0
            pleat_scale = 1.0 - (s_idx / max(1, len(slices) - 1)) * 0.65
            ring = []
            for i in range(seg):
                ang = (2.0 * math.pi * i) / seg
                ca = math.cos(ang)
                sa = math.sin(ang)
                pleat = (1.0 + pleat_amp * pleat_scale * math.cos(pleat_freq * ang)) if pleat_freq > 0 else 1.0
                y_extra = f_bulge * max(0.0, sa) ** 2
                ring.append((cx + ca * rx * pleat, cy + sa * ry * pleat + y_extra, z))
            rings.append(ring)
        self.add_ring_loft(rings, close_bottom=close_bottom, close_top=close_top)

    def add_x_loft(self, slices, seg=12, close_back=True, close_front=True):
        """slices along X axis: list of (x, ry, rz, cy, cz)"""
        rings = []
        for x, ry, rz, cy, cz in slices:
            ring = []
            for i in range(seg):
                ang = (2.0 * math.pi * i) / seg
                ca = math.cos(ang)
                sa = math.sin(ang)
                # CCW around +X: (x, cy + ca*ry, cz + sa*rz)
                ring.append((x, cy + ca * ry, cz + sa * rz))
            rings.append(ring)
        self.add_ring_loft(rings, close_bottom=close_back, close_top=close_front)

    def add_ellipsoid(self, cx, cy, cz, rx, ry, rz, lat_seg=7, lon_seg=12):
        slices = []
        for i in range(lat_seg + 1):
            t = i / lat_seg
            # avoid exact 0 radius at poles because close_bottom/close_top closes them
            ang = (t - 0.5) * math.pi * 0.88
            z = cz + math.sin(ang) * rz
            scale = math.cos(ang)
            slices.append((z, rx * scale, ry * scale, cx, cy))
        self.add_z_loft(slices, seg=lon_seg, close_bottom=True, close_top=True)

    def add_tapered_rod(self, p0, p1, r0, r1, seg=8):
        dx, dy, dz = v_sub(p1, p0)
        length = math.sqrt(dx * dx + dy * dy + dz * dz)
        if length < 1e-6:
            return
        axis = (dx / length, dy / length, dz / length)
        ref = (1.0, 0.0, 0.0) if abs(axis[0]) < 0.85 else (0.0, 1.0, 0.0)
        u = v_norm(v_cross(axis, ref))
        v = v_norm(v_cross(axis, u))
        rings = []
        for t, r in [(0.0, r0), (0.5, (r0 + r1) * 0.52), (1.0, r1)]:
            px = p0[0] + dx * t
            py = p0[1] + dy * t
            pz = p0[2] + dz * t
            ring = []
            for i in range(seg):
                ang = (2.0 * math.pi * i) / seg
                ca = math.cos(ang)
                sa = math.sin(ang)
                ring.append((
                    px + (u[0] * ca + v[0] * sa) * r,
                    py + (u[1] * ca + v[1] * sa) * r,
                    pz + (u[2] * ca + v[2] * sa) * r,
                ))
            rings.append(ring)
        self.add_ring_loft(rings, close_bottom=True, close_top=True)

    def compute_normals(self):
        normals = [[0.0, 0.0, 0.0] for _ in range(len(self.positions))]
        for i in range(0, len(self.indices), 3):
            ia, ib, ic = self.indices[i], self.indices[i + 1], self.indices[i + 2]
            pa, pb, pc = self.positions[ia], self.positions[ib], self.positions[ic]
            fn = v_cross(v_sub(pb, pa), v_sub(pc, pa))
            for idx in (ia, ib, ic):
                normals[idx][0] += fn[0]
                normals[idx][1] += fn[1]
                normals[idx][2] += fn[2]
        return [v_norm(n) for n in normals]


# Material indices for humanoids:
# 0: cloth (primary tunic/robe - tinted by profile BaseColor in engine)
# 1: skin (natural face/hands/neck skin)
# 2: leather (boots, belt, bracers, straps)
# 3: metal (buckle, weapon blade, ornaments)
# 4: hair (hair, beard, braids, eyebrows, antlers/horns)
# 5: trim (trousers, embroidery collar/hem, eyes/accents)

HUMANOID_PALETTES = {
    "player_hunter_animated.glb": {
        "cloth": [0.22, 0.34, 0.16, 1.0],
        "skin": [0.76, 0.57, 0.45, 1.0],
        "leather": [0.23, 0.12, 0.06, 1.0],
        "metal": [0.64, 0.67, 0.70, 1.0],
        "hair": [0.19, 0.12, 0.07, 1.0],
        "trim": [0.28, 0.23, 0.17, 1.0],
        "style": "hunter",
    },
    "npc_hunter_animated.glb": {
        "cloth": [0.20, 0.31, 0.15, 1.0],
        "skin": [0.74, 0.55, 0.43, 1.0],
        "leather": [0.21, 0.11, 0.05, 1.0],
        "metal": [0.60, 0.62, 0.65, 1.0],
        "hair": [0.24, 0.15, 0.08, 1.0],
        "trim": [0.26, 0.22, 0.16, 1.0],
        "style": "npc_hunter",
    },
    "npc_villager_a_animated.glb": {
        "cloth": [0.42, 0.28, 0.16, 1.0],
        "skin": [0.77, 0.58, 0.46, 1.0],
        "leather": [0.24, 0.13, 0.07, 1.0],
        "metal": [0.52, 0.50, 0.46, 1.0],
        "hair": [0.36, 0.24, 0.12, 1.0],
        "trim": [0.30, 0.26, 0.20, 1.0],
        "style": "villager_m",
    },
    "npc_villager_b_animated.glb": {
        "cloth": [0.38, 0.26, 0.18, 1.0],
        "skin": [0.80, 0.61, 0.49, 1.0],
        "leather": [0.25, 0.14, 0.08, 1.0],
        "metal": [0.58, 0.52, 0.38, 1.0],
        "hair": [0.28, 0.16, 0.09, 1.0],
        "trim": [0.58, 0.48, 0.36, 1.0],
        "style": "villager_f",
    },
    "npc_merchant_animated.glb": {
        "cloth": [0.48, 0.32, 0.17, 1.0],
        "skin": [0.76, 0.57, 0.46, 1.0],
        "leather": [0.26, 0.13, 0.06, 1.0],
        "metal": [0.74, 0.60, 0.26, 1.0],
        "hair": [0.16, 0.11, 0.08, 1.0],
        "trim": [0.56, 0.22, 0.14, 1.0],
        "style": "merchant",
    },
    "npc_elder_animated.glb": {
        "cloth": [0.34, 0.31, 0.27, 1.0],
        "skin": [0.72, 0.54, 0.43, 1.0],
        "leather": [0.20, 0.11, 0.06, 1.0],
        "metal": [0.56, 0.54, 0.48, 1.0],
        "hair": [0.68, 0.67, 0.64, 1.0],
        "trim": [0.46, 0.38, 0.26, 1.0],
        "style": "elder",
    },
    "topielec_animated.glb": {
        "cloth": [0.16, 0.28, 0.22, 1.0],
        "skin": [0.34, 0.48, 0.40, 1.0],
        "leather": [0.12, 0.19, 0.14, 1.0],
        "metal": [0.42, 0.62, 0.48, 1.0],
        "hair": [0.09, 0.18, 0.13, 1.0],
        "trim": [0.22, 0.38, 0.28, 1.0],
        "style": "topielec",
    },
    "poludnica_animated.glb": {
        "cloth": [0.82, 0.76, 0.58, 1.0],
        "skin": [0.70, 0.66, 0.58, 1.0],
        "leather": [0.38, 0.30, 0.18, 1.0],
        "metal": [0.78, 0.72, 0.48, 1.0],
        "hair": [0.76, 0.64, 0.32, 1.0],
        "trim": [0.64, 0.54, 0.34, 1.0],
        "style": "poludnica",
    },
    "upior_animated.glb": {
        "cloth": [0.22, 0.18, 0.24, 1.0],
        "skin": [0.52, 0.48, 0.50, 1.0],
        "leather": [0.15, 0.10, 0.12, 1.0],
        "metal": [0.48, 0.46, 0.52, 1.0],
        "hair": [0.14, 0.12, 0.15, 1.0],
        "trim": [0.46, 0.16, 0.16, 1.0],
        "style": "upior",
    },
    "boginka_animated.glb": {
        "cloth": [0.22, 0.36, 0.26, 1.0],
        "skin": [0.48, 0.56, 0.48, 1.0],
        "leather": [0.16, 0.22, 0.15, 1.0],
        "metal": [0.46, 0.58, 0.44, 1.0],
        "hair": [0.14, 0.26, 0.18, 1.0],
        "trim": [0.34, 0.48, 0.32, 1.0],
        "style": "boginka",
    },
    "forest_guardian_animated.glb": {
        "cloth": [0.22, 0.32, 0.16, 1.0],
        "skin": [0.32, 0.23, 0.14, 1.0],
        "leather": [0.18, 0.12, 0.06, 1.0],
        "metal": [0.58, 0.52, 0.30, 1.0],
        "hair": [0.26, 0.20, 0.11, 1.0],
        "trim": [0.36, 0.46, 0.20, 1.0],
        "style": "leszy",
    },
}


def build_humanoid_meshes(style, has_weapon, nodes):
    meshes = []

    root_z = nodes.get("root", [0.0, 0.0, 0.76])[2]
    spine_z = nodes.get("spine", [0.0, 0.0, 0.18])[2]
    head_z = nodes.get("head", [0.0, 0.0, 0.65])[2]
    arm_x = abs(nodes.get("upper_arm_L", [-0.33, 0.0, 0.52])[0])
    arm_z = nodes.get("upper_arm_L", [-0.33, 0.0, 0.52])[2]
    forearm_z = nodes.get("forearm_L", [0.0, 0.0, -0.32])[2]
    thigh_x = abs(nodes.get("thigh_L", [-0.09, 0.0, 0.0])[0])
    shin_z = nodes.get("shin_L", [0.0, 0.0, -0.40])[2]
    foot_z = nodes.get("foot_L", [0.0, 0.0, -0.37])[2]

    # 0: pelvis_mesh (hips + pleated Slavic tunic skirt/zapaska + leather belt + buckle + belt knife/pouch)
    cloth_p = PartBuilder(0)
    leather_p = PartBuilder(2)
    metal_p = PartBuilder(3)
    trim_p = PartBuilder(5)

    is_robed = style in ("elder", "poludnica", "villager_f")
    skirt_drop = -0.26 if is_robed else -0.16
    skirt_flare = 1.28 if is_robed else 1.15
    hip_w = max(0.195, thigh_x * 2.08)
    waist_w = hip_w * (0.92 if style != "merchant" else 1.04)

    cloth_p.add_z_loft([
        (skirt_drop, hip_w * skirt_flare, 0.148 * skirt_flare, 0.0, 0.002),
        (skirt_drop * 0.55, hip_w * (1.0 + (skirt_flare - 1.0) * 0.55), 0.140, 0.0, 0.004),
        (-0.02, hip_w * 1.02, 0.136, 0.0, 0.005),
        (0.09, waist_w, 0.128, 0.0, 0.004),
        (spine_z + 0.04, waist_w * 0.98, 0.126, 0.0, 0.004),
    ], seg=16, pleat_freq=8, pleat_amp=0.032)

    # Embroidered Slavic hem border + woven apron/zapaska panel on villager_f
    trim_p.add_z_loft([
        (skirt_drop - 0.014, hip_w * skirt_flare * 1.012, 0.150 * skirt_flare, 0.0, 0.002),
        (skirt_drop + 0.028, hip_w * skirt_flare * 1.008, 0.149 * skirt_flare, 0.0, 0.002),
    ], seg=16, pleat_freq=8, pleat_amp=0.028)

    if style == "villager_f":
        # Traditional layered front zapaska apron panel with folk trim
        trim_p.add_z_loft([
            (skirt_drop + 0.02, hip_w * 0.76, 0.032, 0.0, 0.142),
            (-0.04, hip_w * 0.68, 0.028, 0.0, 0.132),
            (0.09, hip_w * 0.60, 0.024, 0.0, 0.124),
        ], seg=12)

    # Thick stitched leather belt + side utility knife sheath
    leather_p.add_z_loft([
        (0.085, waist_w * 1.045, 0.136, 0.0, 0.005),
        (0.162, waist_w * 1.040, 0.135, 0.0, 0.005),
    ], seg=16)
    leather_p.add_tapered_rod((waist_w * 0.92, 0.06, 0.12), (waist_w * 0.98, -0.02, -0.06), 0.024, 0.012, seg=8)
    metal_p.add_ellipsoid(0.0, 0.142, 0.124, 0.040, 0.016, 0.036, lat_seg=5, lon_seg=10)
    meshes.append(("pelvis_mesh", [cloth_p, trim_p, leather_p, metal_p]))

    # 1: torso_mesh (anatomical ribcage, pectorals, clavicle/deltoid shoulder bridge reaching ±arm_x, collar)
    cloth_t = PartBuilder(0)
    skin_t = PartBuilder(1)
    leather_t = PartBuilder(2)
    metal_t = PartBuilder(3)
    trim_t = PartBuilder(5)

    # Ensure torso shoulders reach the exact upper_arm_L/R joint (±arm_x) so arms never float!
    shoulder_span = max(arm_x * 0.96, 0.255)
    chest_d = 0.152 if style != "merchant" else 0.172
    belly_bulge = 0.014 if style != "merchant" else 0.034
    chest_bulge = 0.034 if style == "villager_f" else 0.024

    cloth_t.add_z_loft([
        (-0.02, waist_w * 0.98, 0.124, 0.0, 0.002, belly_bulge * 0.6),
        (head_z * 0.26, waist_w * 1.06, chest_d * 0.90, 0.0, 0.006, belly_bulge),
        (head_z * 0.56, shoulder_span * 0.84, chest_d, 0.0, 0.010, chest_bulge),
        (arm_z, shoulder_span, chest_d * 0.96, 0.0, 0.005, 0.016),
        (head_z * 0.92, shoulder_span * 0.66, 0.112, 0.0, 0.002, 0.006),
        (head_z + 0.01, 0.095, 0.092, 0.0, 0.002, 0.0),
    ], seg=16, pleat_freq=6, pleat_amp=0.018)

    # Sculpted deltoid shoulder caps bridging torso to ±arm_x at z = arm_z
    for side in (-1.0, 1.0):
        cloth_t.add_tapered_rod(
            (side * (shoulder_span * 0.68), 0.0, arm_z + 0.015),
            (side * (arm_x + 0.012), 0.0, arm_z - 0.010),
            0.082,
            0.070,
            seg=12,
        )

    # Embroidered Slavic V-neck collar & tunic placket (koszula słowiańska)
    trim_t.add_z_loft([
        (head_z * 0.84, 0.122, 0.108, 0.0, 0.012),
        (head_z + 0.025, 0.086, 0.086, 0.0, 0.006),
    ], seg=12)
    trim_t.add_tapered_rod((0.0, chest_d + 0.014, head_z * 0.88), (0.0, chest_d + 0.018, head_z * 0.52), 0.022, 0.018, seg=8)

    # Anatomical neck column with sternocleidomastoid taper connecting torso into head
    skin_t.add_z_loft([
        (head_z * 0.90, 0.072, 0.072, 0.0, 0.006),
        (head_z + 0.055, 0.062, 0.064, 0.0, 0.008),
    ], seg=12)

    if style in ("hunter", "npc_hunter"):
        # Fur-trimmed shoulder mantle + cross-body leather baldric & chest buckle
        for side in (-1.0, 1.0):
            leather_t.add_ellipsoid(side * (arm_x * 0.82), 0.0, arm_z + 0.025, 0.095, 0.092, 0.052, lat_seg=5, lon_seg=10)
        leather_t.add_tapered_rod((-arm_x * 0.72, 0.02, arm_z + 0.04), (waist_w * 0.88, 0.132, 0.05), 0.028, 0.024, seg=8)
        leather_t.add_tapered_rod((-arm_x * 0.72, -0.02, arm_z + 0.04), (waist_w * 0.88, -0.125, 0.05), 0.028, 0.024, seg=8)
        metal_t.add_ellipsoid(-0.04, chest_d + 0.018, head_z * 0.58, 0.024, 0.012, 0.024, lat_seg=4, lon_seg=8)
        # Slavic hunter equipment: Perun solar bronze talisman, double belt pouches, knife sheath & krajka belt hem
        metal_t.add_ellipsoid(0.0, chest_d + 0.019, head_z * 0.65, 0.026, 0.008, 0.026, lat_seg=5, lon_seg=10)
        leather_t.add_ellipsoid(waist_w * 0.86, 0.035, 0.025, 0.036, 0.030, 0.044, lat_seg=4, lon_seg=8)
        metal_t.add_ellipsoid(waist_w * 0.86, 0.065, 0.025, 0.010, 0.006, 0.010, lat_seg=3, lon_seg=6)
        leather_t.add_tapered_rod((-waist_w * 0.84, 0.03, 0.05), (-waist_w * 0.86, 0.01, -0.15), 0.018, 0.012, seg=6)
        metal_t.add_ellipsoid(-waist_w * 0.84, 0.03, 0.07, 0.014, 0.008, 0.014, lat_seg=3, lon_seg=6)
        trim_t.add_z_loft([
            (0.012, waist_w * 1.03, 0.128, 0.0, 0.0),
            (0.052, waist_w * 1.04, 0.130, 0.0, 0.0),
        ], seg=14)
    elif style == "merchant":
        # Rich żupan coat lapels + ornamental brass clasps
        for side in (-1.0, 1.0):
            trim_t.add_tapered_rod((side * 0.085, chest_d * 0.95, head_z * 0.88), (side * 0.115, chest_d * 0.92, 0.04), 0.036, 0.040, seg=8)
        for z_clasp in (head_z * 0.42, head_z * 0.60, head_z * 0.76):
            metal_t.add_ellipsoid(0.0, chest_d + 0.022, z_clasp, 0.026, 0.010, 0.012, lat_seg=4, lon_seg=8)
    elif style == "villager_f":
        # Red/amber Slavic coral necklace (korale) around collar
        for ang_deg in range(-65, 66, 26):
            rad = math.radians(ang_deg)
            bx = math.sin(rad) * 0.095
            by = math.cos(rad) * 0.105 + 0.012
            bz = head_z * 0.84 - (1.0 - math.cos(rad)) * 0.045
            metal_t.add_ellipsoid(bx, by, bz, 0.014, 0.014, 0.014, lat_seg=4, lon_seg=6)
    elif style == "elder":
        # Draped elder cloak mantle + bronze solar amulet on chest
        leather_t.add_z_loft([
            (arm_z - 0.06, shoulder_span * 1.05, chest_d * 1.08, 0.0, -0.008),
            (head_z * 0.92, shoulder_span * 0.78, 0.128, 0.0, -0.006),
        ], seg=14)
        metal_t.add_ellipsoid(0.0, chest_d + 0.022, head_z * 0.64, 0.028, 0.010, 0.028, lat_seg=5, lon_seg=10)
    elif style == "leszy":
        for side in (-1.0, 1.0):
            trim_t.add_tapered_rod((side * (arm_x * 0.65), 0.0, arm_z + 0.02), (side * (arm_x * 1.15), -0.02, arm_z + 0.14), 0.072, 0.020, seg=8)
    elif style == "topielec":
        for zf in (0.18, 0.34, 0.50):
            trim_t.add_tapered_rod((0.0, -0.11, zf), (0.0, -0.22, zf + 0.08), 0.026, 0.006, seg=6)

    meshes.append(("torso_mesh", [p for p in (cloth_t, trim_t, skin_t, leather_t, metal_t) if p.positions]))

    # 2: head_mesh (detailed facial anatomy: cranium, jaw, cheekbones, brow ridge, nose, lips, ears, eyes, hair/headgear)
    cloth_h = PartBuilder(0)
    skin_h = PartBuilder(1)
    leather_h = PartBuilder(2)
    metal_h = PartBuilder(3)
    hair_h = PartBuilder(4)
    trim_h = PartBuilder(5)

    # Sculpted head with chin, jawline, cheekbones, brow ridge, and cranium
    skin_h.add_z_loft([
        (-0.025, 0.058, 0.060, 0.0, 0.002),
        (0.040, 0.064, 0.068, 0.0, 0.008),
        (0.078, 0.082, 0.104, 0.0, 0.026, 0.018),  # sculpted chin
        (0.125, 0.106, 0.120, 0.0, 0.020, 0.014),  # jawline & mouth level
        (0.175, 0.116, 0.126, 0.0, 0.014, 0.012),  # cheekbones (jarzmowe)
        (0.225, 0.118, 0.132, 0.0, 0.010, 0.014),  # prominent brow ridge
        (0.285, 0.112, 0.124, 0.0, 0.000, 0.004),  # forehead
        (0.335, 0.086, 0.096, 0.0, -0.006, 0.0),   # crown
        (0.358, 0.048, 0.054, 0.0, -0.008, 0.0),
    ], seg=16)

    # Cheekbone & fuller jaw volume
    for side in (-1.0, 1.0):
        skin_h.add_ellipsoid(side * 0.068, 0.088, 0.168, 0.042, 0.036, 0.034, lat_seg=4, lon_seg=8)
        # Sculpted ears with helix & lobe
        skin_h.add_ellipsoid(side * 0.118, -0.004, 0.188, 0.018, 0.026, 0.036, lat_seg=5, lon_seg=8)

    # Anatomical nose: bridge + tip + nostrils
    skin_h.add_tapered_rod((0.0, 0.122, 0.224), (0.0, 0.156, 0.164), 0.014, 0.021, seg=8)
    for side in (-1.0, 1.0):
        skin_h.add_ellipsoid(side * 0.014, 0.144, 0.162, 0.010, 0.010, 0.008, lat_seg=4, lon_seg=6)

    # Subtle sculpted lips
    skin_h.add_tapered_rod((-0.026, 0.132, 0.128), (0.026, 0.132, 0.128), 0.010, 0.010, seg=6)

    # Eyes (sclera + dark iris) and arched eyebrows
    for side in (-1.0, 1.0):
        trim_h.add_ellipsoid(side * 0.044, 0.124, 0.206, 0.015, 0.011, 0.011, lat_seg=5, lon_seg=8)
        hair_h.add_tapered_rod((side * 0.022, 0.130, 0.228), (side * 0.068, 0.118, 0.224), 0.008, 0.007, seg=6)

    # Full volume hair cap covering back, temples, and crown
    hair_h.add_z_loft([
        (0.08, 0.106, 0.098, 0.0, -0.036),
        (0.16, 0.122, 0.120, 0.0, -0.024),
        (0.24, 0.126, 0.132, 0.0, -0.012),
        (0.31, 0.120, 0.130, 0.0, -0.004),
        (0.365, 0.082, 0.090, 0.0, -0.004),
    ], seg=16)

    if style in ("hunter", "npc_hunter", "villager_m"):
        # Full Slavic beard, sideburns & handlebar mustache
        hair_h.add_z_loft([
            (0.045, 0.072, 0.065, 0.0, 0.078),
            (0.085, 0.096, 0.076, 0.0, 0.072),
            (0.130, 0.108, 0.072, 0.0, 0.056),
        ], seg=12)
        for side in (-1.0, 1.0):
            hair_h.add_tapered_rod((0.0, 0.142, 0.142), (side * 0.052, 0.118, 0.126), 0.014, 0.009, seg=6)
    elif style == "merchant":
        # Groomed merchant beard + traditional Slavic fur-trimmed cap (kołpak)
        hair_h.add_z_loft([
            (0.055, 0.074, 0.068, 0.0, 0.076),
            (0.095, 0.098, 0.076, 0.0, 0.070),
            (0.132, 0.106, 0.070, 0.0, 0.054),
        ], seg=12)
        leather_h.add_z_loft([
            (0.245, 0.134, 0.140, 0.0, -0.002),
            (0.295, 0.136, 0.142, 0.0, -0.002),
        ], seg=16)
        cloth_h.add_z_loft([
            (0.285, 0.124, 0.128, 0.0, -0.004),
            (0.365, 0.108, 0.112, 0.0, -0.008),
            (0.415, 0.052, 0.056, 0.0, -0.012),
        ], seg=14)
        metal_h.add_ellipsoid(0.0, 0.140, 0.275, 0.018, 0.010, 0.020, lat_seg=4, lon_seg=8)
    elif style == "elder":
        # Long braided Slavic elder beard + flowing mustache + brow band
        hair_h.add_tapered_rod((0.0, 0.105, 0.130), (0.0, 0.132, -0.03), 0.086, 0.054, seg=10)
        hair_h.add_tapered_rod((0.0, 0.132, -0.02), (0.0, 0.142, -0.14), 0.052, 0.022, seg=8)
        for side in (-1.0, 1.0):
            hair_h.add_tapered_rod((0.0, 0.142, 0.144), (side * 0.064, 0.116, 0.112), 0.016, 0.009, seg=6)
        trim_h.add_z_loft([
            (0.248, 0.124, 0.134, 0.0, -0.002),
            (0.272, 0.125, 0.135, 0.0, -0.002),
        ], seg=14)
    elif style in ("villager_f", "poludnica"):
        # Twin Slavic braids draping forward/back + embroidered headscarf/wreath (wianek/chustka)
        hair_h.add_tapered_rod((0.0, -0.112, 0.17), (0.0, -0.138, -0.09), 0.046, 0.022, seg=8)
        for side in (-1.0, 1.0):
            hair_h.add_tapered_rod((side * 0.088, 0.015, 0.14), (side * 0.098, 0.065, -0.06), 0.028, 0.016, seg=8)
        trim_h.add_z_loft([
            (0.245, 0.130, 0.138, 0.0, -0.002),
            (0.285, 0.132, 0.140, 0.0, -0.002),
        ], seg=16, pleat_freq=10, pleat_amp=0.025)
    elif style == "leszy":
        for side in (-1.0, 1.0):
            hair_h.add_tapered_rod((side * 0.07, 0.01, 0.32), (side * 0.22, 0.04, 0.52), 0.024, 0.012, seg=6)
            hair_h.add_tapered_rod((side * 0.15, 0.03, 0.42), (side * 0.28, 0.12, 0.56), 0.018, 0.008, seg=6)
            hair_h.add_tapered_rod((side * 0.19, 0.03, 0.47), (side * 0.16, -0.06, 0.62), 0.016, 0.007, seg=6)

    meshes.append(("head_mesh", [p for p in (skin_h, hair_h, trim_h, leather_h, cloth_h, metal_h) if p.positions]))

    # 3: upperarm_mesh (z = [forearm_z - 0.02 .. +0.05] with shoulder ball & elbow overlap)
    cloth_ua = PartBuilder(0)
    trim_ua = PartBuilder(5)
    ua_bottom = forearm_z - 0.022
    cloth_ua.add_z_loft([
        (ua_bottom, 0.054, 0.052, 0.0, 0.0),
        (forearm_z * 0.58, 0.066, 0.064, 0.0, 0.003),
        (forearm_z * 0.20, 0.076, 0.074, 0.0, 0.002),
        (0.015, 0.078, 0.076, 0.0, 0.0),
        (0.052, 0.058, 0.058, 0.0, 0.0),
    ], seg=12, pleat_freq=5, pleat_amp=0.018)
    trim_ua.add_z_loft([
        (forearm_z - 0.012, 0.058, 0.056, 0.0, 0.0),
        (forearm_z + 0.028, 0.060, 0.058, 0.0, 0.0),
    ], seg=12)
    meshes.append(("upperarm_mesh", [cloth_ua, trim_ua]))

    # 4: forearm_mesh (leather bracer + sculpted wrist, palm, thumb, and fingers)
    leather_fa = PartBuilder(2)
    skin_fa = PartBuilder(1)
    wrist_z = forearm_z * 0.66
    hand_tip_z = forearm_z * 0.99
    leather_fa.add_z_loft([
        (wrist_z, 0.044, 0.042, 0.0, 0.0),
        (forearm_z * 0.30, 0.056, 0.054, 0.0, -0.002),
        (0.022, 0.054, 0.052, 0.0, 0.0),
    ], seg=12)
    # Sculpted hand palm + fingers + thumb
    skin_fa.add_z_loft([
        (hand_tip_z, 0.032, 0.018, 0.0, 0.006),
        ((wrist_z + hand_tip_z) * 0.52, 0.046, 0.026, 0.0, 0.006),
        (wrist_z + 0.012, 0.040, 0.036, 0.0, 0.002),
    ], seg=10)
    skin_fa.add_tapered_rod(
        (0.028, 0.012, wrist_z - 0.015),
        (0.046, 0.032, (wrist_z + hand_tip_z) * 0.55),
        0.013,
        0.009,
        seg=6,
    )
    meshes.append(("forearm_mesh", [leather_fa, skin_fa]))

    # 5: thigh_mesh (spans from hip socket +0.045 down to knee shin_z - 0.025)
    trim_th = PartBuilder(5)
    trim_th.add_z_loft([
        (shin_z - 0.025, 0.066, 0.066, 0.0, 0.004),
        (shin_z * 0.55, 0.086, 0.084, 0.0, 0.006),
        (shin_z * 0.16, 0.098, 0.096, 0.0, 0.002),
        (0.045, 0.084, 0.082, 0.0, 0.0),
    ], seg=12, pleat_freq=5, pleat_amp=0.016)
    meshes.append(("thigh_mesh", [trim_th]))

    # 6: shin_mesh (spans from knee +0.025 down to ankle foot_z - 0.015 with Slavic leg wraps)
    trim_sh = PartBuilder(5)
    leather_sh = PartBuilder(2)
    trim_sh.add_z_loft([
        (foot_z - 0.015, 0.050, 0.050, 0.0, 0.0),
        (foot_z * 0.62, 0.058, 0.060, 0.0, -0.004),
        (foot_z * 0.28, 0.070, 0.072, 0.0, -0.008),
        (0.025, 0.066, 0.066, 0.0, 0.002),
    ], seg=12)
    for frac in (0.82, 0.56, 0.32):
        zb = foot_z * frac
        leather_sh.add_z_loft([
            (zb - 0.018, 0.061, 0.063, 0.0, -0.004),
            (zb + 0.018, 0.062, 0.064, 0.0, -0.004),
        ], seg=12)
    meshes.append(("shin_mesh", [trim_sh, leather_sh]))

    # 7: foot_mesh (sculpted leather boot grounded cleanly at sole_z)
    leather_ft = PartBuilder(2)
    sole_z = -(root_z + shin_z + foot_z)
    sole_z = min(-0.075, max(-0.115, sole_z))
    leather_ft.add_z_loft([
        (sole_z, 0.068, 0.145, 0.0, 0.076),
        (sole_z * 0.68, 0.072, 0.150, 0.0, 0.080),
        (sole_z * 0.30, 0.060, 0.112, 0.0, 0.042),
        (0.022, 0.054, 0.062, 0.0, 0.004),
    ], seg=12)
    meshes.append(("foot_mesh", [leather_ft]))

    # 8: weapon_mesh (forged Slavic sword with pommel, grip, crossguard, and blade)
    if has_weapon:
        leather_wp = PartBuilder(2)
        metal_wp = PartBuilder(3)
        # Pommel
        metal_wp.add_ellipsoid(0.0, 0.0, 0.035, 0.024, 0.020, 0.022, lat_seg=4, lon_seg=8)
        # Leather grip
        leather_wp.add_z_loft([
            (-0.115, 0.016, 0.013, 0.0, 0.0),
            (0.020, 0.017, 0.014, 0.0, 0.0),
        ], seg=8)
        # Crossguard
        metal_wp.add_tapered_rod((-0.075, 0.0, -0.125), (0.075, 0.0, -0.125), 0.016, 0.016, seg=6)
        # Double-edged fuller blade
        metal_wp.add_z_loft([
            (-0.845, 0.003, 0.002, 0.0, 0.0),
            (-0.740, 0.022, 0.006, 0.0, 0.0),
            (-0.420, 0.026, 0.007, 0.0, 0.0),
            (-0.130, 0.030, 0.008, 0.0, 0.0),
        ], seg=8)
        meshes.append(("weapon_mesh", [leather_wp, metal_wp]))

    return meshes


QUADRUPED_PALETTES = {
    "swamp_predator_animated.glb": {
        "body": [0.20, 0.25, 0.17, 1.0],
        "limb": [0.14, 0.18, 0.12, 1.0],
        "leather": [0.10, 0.13, 0.09, 1.0],
        "metal": [0.85, 0.68, 0.22, 1.0],
        "hair": [0.12, 0.16, 0.10, 1.0],
        "trim": [0.34, 0.42, 0.24, 1.0],
        "species": "predator",
    },
    "wolf_animated.glb": {
        "body": [0.28, 0.26, 0.24, 1.0],
        "limb": [0.38, 0.35, 0.31, 1.0],
        "leather": [0.12, 0.11, 0.10, 1.0],
        "metal": [0.78, 0.62, 0.20, 1.0],
        "hair": [0.18, 0.16, 0.15, 1.0],
        "trim": [0.52, 0.49, 0.45, 1.0],
        "species": "wolf",
    },
    "deer_animated.glb": {
        "body": [0.46, 0.30, 0.17, 1.0],
        "limb": [0.36, 0.23, 0.13, 1.0],
        "leather": [0.15, 0.11, 0.08, 1.0],
        "metal": [0.18, 0.15, 0.12, 1.0],
        "hair": [0.58, 0.48, 0.34, 1.0],
        "trim": [0.72, 0.64, 0.52, 1.0],
        "species": "deer",
    },
    "boar_animated.glb": {
        "body": [0.24, 0.17, 0.12, 1.0],
        "limb": [0.17, 0.12, 0.08, 1.0],
        "leather": [0.12, 0.09, 0.07, 1.0],
        "metal": [0.86, 0.82, 0.72, 1.0],
        "hair": [0.14, 0.10, 0.07, 1.0],
        "trim": [0.32, 0.24, 0.18, 1.0],
        "species": "boar",
    },
}


def build_quadruped_meshes(species):
    # Quadrupeds are authored with +X forward, -X tail, Y left/right, +Z up
    body_p = PartBuilder(0)
    limb_p = PartBuilder(1)
    hair_p = PartBuilder(4)
    trim_p = PartBuilder(5)

    if species == "deer":
        body_p.add_x_loft([
            (-0.52, 0.15, 0.16, 0.0, 0.02),
            (-0.32, 0.20, 0.22, 0.0, 0.03),
            (0.00, 0.21, 0.23, 0.0, 0.00),
            (0.32, 0.22, 0.25, 0.0, 0.04),
            (0.52, 0.16, 0.20, 0.0, 0.10),
        ], seg=12)
    elif species == "boar":
        body_p.add_x_loft([
            (-0.50, 0.18, 0.18, 0.0, -0.02),
            (-0.24, 0.25, 0.25, 0.0, 0.00),
            (0.10, 0.28, 0.29, 0.0, 0.04),
            (0.42, 0.24, 0.26, 0.0, 0.02),
            (0.54, 0.18, 0.19, 0.0, 0.00),
        ], seg=12)
        # Dorsal bristle mane
        hair_p.add_tapered_rod((-0.38, 0.0, 0.22), (0.46, 0.0, 0.27), 0.035, 0.055, seg=6)
    else:  # wolf or predator
        body_p.add_x_loft([
            (-0.52, 0.16, 0.17, 0.0, 0.02),
            (-0.28, 0.20, 0.21, 0.0, 0.02),
            (0.04, 0.21, 0.23, 0.0, -0.01),
            (0.34, 0.24, 0.26, 0.0, 0.03),
            (0.53, 0.19, 0.21, 0.0, 0.05),
        ], seg=12)
        if species == "predator":
            for xf in (-0.30, -0.08, 0.16, 0.36):
                trim_p.add_tapered_rod((xf, 0.0, 0.20), (xf - 0.08, 0.0, 0.33), 0.035, 0.008, seg=6)

    body_parts = [p for p in (body_p, hair_p, trim_p) if p.positions]

    # Head mesh (+X forward)
    head_main = PartBuilder(0)
    head_detail = PartBuilder(4)
    head_accent = PartBuilder(3)
    head_main.add_x_loft([
        (-0.04, 0.14, 0.14, 0.0, 0.02),
        (0.10, 0.16, 0.15, 0.0, 0.02),
        (0.24, 0.11, 0.10, 0.0, -0.01),
        (0.39, 0.065, 0.062, 0.0, -0.02),
    ], seg=10)
    # Ears
    for sy in (-1.0, 1.0):
        head_main.add_tapered_rod((0.04, sy * 0.11, 0.11), (-0.01, sy * 0.16, 0.25), 0.042, 0.010, seg=6)
        head_accent.add_ellipsoid(0.18, sy * 0.10, 0.06, 0.018, 0.016, 0.016, lat_seg=4, lon_seg=6)

    if species == "deer":
        # Branching stag antlers
        for sy in (-1.0, 1.0):
            head_detail.add_tapered_rod((0.05, sy * 0.07, 0.14), (-0.12, sy * 0.26, 0.44), 0.022, 0.010, seg=6)
            head_detail.add_tapered_rod((-0.02, sy * 0.15, 0.27), (0.10, sy * 0.24, 0.40), 0.016, 0.007, seg=6)
            head_detail.add_tapered_rod((-0.07, sy * 0.21, 0.35), (-0.04, sy * 0.16, 0.50), 0.015, 0.007, seg=6)
    elif species == "boar":
        # Curved white boar tusks
        for sy in (-1.0, 1.0):
            head_accent.add_tapered_rod((0.28, sy * 0.08, -0.03), (0.36, sy * 0.13, 0.08), 0.020, 0.006, seg=6)

    head_parts = [p for p in (head_main, head_detail, head_accent) if p.positions]

    # Leg mesh (z = [-0.58 .. 0.0])
    leg_p = PartBuilder(1)
    hoof_p = PartBuilder(2)
    leg_p.add_z_loft([
        (-0.52, 0.038, 0.038, 0.00, 0.0),
        (-0.32, 0.046, 0.044, -0.01, 0.0),
        (-0.14, 0.062, 0.056, 0.01, 0.0),
        (0.01, 0.068, 0.060, 0.00, 0.0),
    ], seg=10)
    hoof_p.add_z_loft([
        (-0.58, 0.046, 0.044, 0.018, 0.0),
        (-0.51, 0.040, 0.040, 0.005, 0.0),
    ], seg=10)

    # Tail mesh (z = [-0.55 .. 0.0])
    tail_p = PartBuilder(0)
    tail_r = 0.065 if species == "wolf" else 0.048
    tail_p.add_z_loft([
        (-0.55, 0.014, 0.014, 0.0, 0.0),
        (-0.34, tail_r, tail_r, 0.0, 0.0),
        (-0.14, tail_r * 0.9, tail_r * 0.9, 0.0, 0.0),
        (0.01, 0.038, 0.038, 0.0, 0.0),
    ], seg=8)

    return [
        ("body_mesh", body_parts),
        ("head_mesh", head_parts),
        ("leg_mesh", [leg_p, hoof_p]),
        ("tail_mesh", [tail_p]),
    ]


def build_raven_meshes():
    # Raven: +X forward, Y left/right, +Z up
    body_p = PartBuilder(0)
    body_p.add_x_loft([
        (-0.30, 0.08, 0.03, 0.0, -0.02),
        (-0.16, 0.12, 0.10, 0.0, 0.00),
        (0.04, 0.15, 0.14, 0.0, 0.00),
        (0.22, 0.12, 0.12, 0.0, 0.02),
        (0.29, 0.07, 0.07, 0.0, 0.03),
    ], seg=10)

    wing_p = PartBuilder(0)
    # Cambered wing extending along +X in local wing node space
    wing_p.add_x_loft([
        (-0.02, 0.032, 0.095, 0.0, 0.0),
        (0.16, 0.026, 0.115, 0.006, -0.01),
        (0.34, 0.018, 0.090, 0.004, -0.02),
        (0.47, 0.008, 0.042, 0.0, -0.01),
    ], seg=8)

    head_p = PartBuilder(0)
    head_p.add_ellipsoid(0.0, 0.0, 0.0, 0.102, 0.092, 0.096, lat_seg=6, lon_seg=10)

    beak_p = PartBuilder(1)
    beak_p.add_x_loft([
        (0.00, 0.034, 0.032, 0.0, 0.00),
        (0.09, 0.022, 0.022, 0.0, -0.004),
        (0.185, 0.004, 0.005, 0.0, -0.012),
    ], seg=8)

    return [
        ("body_mesh", [body_p]),
        ("wing_mesh", [wing_p]),
        ("head_mesh", [head_p]),
        ("beak_mesh", [beak_p]),
    ]


def extract_animations(j, bin_data):
    """Extract raw binary blobs and metadata for all animation samplers so they can be re-emitted unchanged."""
    extracted = []
    accessors = j.get("accessors", [])
    buffer_views = j.get("bufferViews", [])
    for anim in j.get("animations", []):
        samplers_out = []
        for samp in anim.get("samplers", []):
            in_acc = accessors[samp["input"]]
            out_acc = accessors[samp["output"]]
            in_bv = buffer_views[in_acc["bufferView"]]
            out_bv = buffer_views[out_acc["bufferView"]]
            in_off = in_bv.get("byteOffset", 0) + in_acc.get("byteOffset", 0)
            out_off = out_bv.get("byteOffset", 0) + out_acc.get("byteOffset", 0)
            in_bytes = bin_data[in_off : in_off + in_bv["byteLength"]]
            out_bytes = bin_data[out_off : out_off + out_bv["byteLength"]]
            samplers_out.append({
                "interpolation": samp.get("interpolation", "LINEAR"),
                "in_acc": in_acc,
                "in_bytes": in_bytes,
                "out_acc": out_acc,
                "out_bytes": out_bytes,
            })
        extracted.append({
            "name": anim["name"],
            "samplers": samplers_out,
            "channels": anim["channels"],
        })
    return extracted


def build_glb(path: Path):
    raw = path.read_bytes()
    magic, ver, length = struct.unpack_from("<III", raw, 0)
    jlen, jtype = struct.unpack_from("<II", raw, 12)
    j = json.loads(raw[20 : 20 + jlen])
    blen, btype = struct.unpack_from("<II", raw, 20 + jlen)
    bin_data = raw[28 + jlen : 28 + jlen + blen]

    anims = extract_animations(j, bin_data)
    fname = path.name

    if fname in HUMANOID_PALETTES:
        pal = HUMANOID_PALETTES[fname]
        has_weapon = any(n.get("name") == "weapon" for n in j["nodes"])
        nodes_map = {n.get("name"): n.get("translation") for n in j["nodes"] if n.get("translation")}
        meshes_spec = build_humanoid_meshes(pal["style"], has_weapon, nodes_map)
        materials = [
            {"name": "cloth", "pbrMetallicRoughness": {"baseColorFactor": pal["cloth"], "metallicFactor": 0.0, "roughnessFactor": 0.82}},
            {"name": "skin", "pbrMetallicRoughness": {"baseColorFactor": pal["skin"], "metallicFactor": 0.0, "roughnessFactor": 0.62}},
            {"name": "leather", "pbrMetallicRoughness": {"baseColorFactor": pal["leather"], "metallicFactor": 0.05, "roughnessFactor": 0.54}},
            {"name": "metal", "pbrMetallicRoughness": {"baseColorFactor": pal["metal"], "metallicFactor": 0.85, "roughnessFactor": 0.28}},
            {"name": "hair", "pbrMetallicRoughness": {"baseColorFactor": pal["hair"], "metallicFactor": 0.0, "roughnessFactor": 0.76}},
            {"name": "trim", "pbrMetallicRoughness": {"baseColorFactor": pal["trim"], "metallicFactor": 0.0, "roughnessFactor": 0.74}},
        ]
    elif fname in QUADRUPED_PALETTES:
        pal = QUADRUPED_PALETTES[fname]
        meshes_spec = build_quadruped_meshes(pal["species"])
        materials = [
            {"name": "body", "pbrMetallicRoughness": {"baseColorFactor": pal["body"], "metallicFactor": 0.0, "roughnessFactor": 0.78}},
            {"name": "limb", "pbrMetallicRoughness": {"baseColorFactor": pal["limb"], "metallicFactor": 0.0, "roughnessFactor": 0.76}},
            {"name": "leather", "pbrMetallicRoughness": {"baseColorFactor": pal["leather"], "metallicFactor": 0.05, "roughnessFactor": 0.48}},
            {"name": "metal", "pbrMetallicRoughness": {"baseColorFactor": pal["metal"], "metallicFactor": 0.20, "roughnessFactor": 0.35}},
            {"name": "hair", "pbrMetallicRoughness": {"baseColorFactor": pal["hair"], "metallicFactor": 0.0, "roughnessFactor": 0.82}},
            {"name": "trim", "pbrMetallicRoughness": {"baseColorFactor": pal["trim"], "metallicFactor": 0.0, "roughnessFactor": 0.72}},
        ]
    elif fname == "raven_animated.glb":
        meshes_spec = build_raven_meshes()
        materials = [
            {"name": "feathers", "pbrMetallicRoughness": {"baseColorFactor": [0.06, 0.06, 0.08, 1.0], "metallicFactor": 0.12, "roughnessFactor": 0.46}},
            {"name": "beak", "pbrMetallicRoughness": {"baseColorFactor": [0.55, 0.48, 0.30, 1.0], "metallicFactor": 0.05, "roughnessFactor": 0.42}},
        ]
    else:
        raise ValueError(f"Unknown animated model: {fname}")

    out_bin = bytearray()
    buffer_views = []
    accessors = []
    out_meshes = []

    def add_buffer_view(data_bytes, target=None):
        while len(out_bin) % 4 != 0:
            out_bin.append(0)
        offset = len(out_bin)
        out_bin.extend(data_bytes)
        bv = {"buffer": 0, "byteOffset": offset, "byteLength": len(data_bytes)}
        if target is not None:
            bv["target"] = target
        idx = len(buffer_views)
        buffer_views.append(bv)
        return idx

    for mesh_name, parts in meshes_spec:
        prims = []
        for part in parts:
            if not part.positions or not part.indices:
                continue
            normals = part.compute_normals()
            pos_bytes = b"".join(struct.pack("<fff", *p) for p in part.positions)
            norm_bytes = b"".join(struct.pack("<fff", *n) for n in normals)
            idx_bytes = b"".join(struct.pack("<I", idx) for idx in part.indices)

            pos_bv = add_buffer_view(pos_bytes, ARRAY_BUFFER)
            norm_bv = add_buffer_view(norm_bytes, ARRAY_BUFFER)
            idx_bv = add_buffer_view(idx_bytes, ELEMENT_ARRAY_BUFFER)

            xs = [p[0] for p in part.positions]
            ys = [p[1] for p in part.positions]
            zs = [p[2] for p in part.positions]

            pos_acc = len(accessors)
            accessors.append({
                "bufferView": pos_bv,
                "componentType": FLOAT,
                "count": len(part.positions),
                "type": "VEC3",
                "min": [min(xs), min(ys), min(zs)],
                "max": [max(xs), max(ys), max(zs)],
            })
            norm_acc = len(accessors)
            accessors.append({
                "bufferView": norm_bv,
                "componentType": FLOAT,
                "count": len(normals),
                "type": "VEC3",
            })
            idx_acc = len(accessors)
            accessors.append({
                "bufferView": idx_bv,
                "componentType": UNSIGNED_INT,
                "count": len(part.indices),
                "type": "SCALAR",
            })
            prims.append({
                "attributes": {"POSITION": pos_acc, "NORMAL": norm_acc},
                "indices": idx_acc,
                "material": part.material_index,
            })
        out_meshes.append({"name": mesh_name, "primitives": prims})

    out_animations = []
    for anim in anims:
        out_samplers = []
        for samp in anim["samplers"]:
            in_bv = add_buffer_view(samp["in_bytes"])
            out_bv = add_buffer_view(samp["out_bytes"])

            in_acc_new = dict(samp["in_acc"])
            in_acc_new["bufferView"] = in_bv
            in_acc_new.pop("byteOffset", None)
            in_acc_idx = len(accessors)
            accessors.append(in_acc_new)

            out_acc_new = dict(samp["out_acc"])
            out_acc_new["bufferView"] = out_bv
            out_acc_new.pop("byteOffset", None)
            out_acc_idx = len(accessors)
            accessors.append(out_acc_new)

            out_samplers.append({
                "input": in_acc_idx,
                "output": out_acc_idx,
                "interpolation": samp["interpolation"],
            })
        out_animations.append({
            "name": anim["name"],
            "samplers": out_samplers,
            "channels": anim["channels"],
        })

    while len(out_bin) % 4 != 0:
        out_bin.append(0)

    out_gltf = {
        "asset": {"version": "2.0", "generator": "SlavicGame Sculpted Character Generator"},
        "scene": j.get("scene", 0),
        "scenes": j["scenes"],
        "nodes": j["nodes"],
        "meshes": out_meshes,
        "materials": materials,
        "buffers": [{"byteLength": len(out_bin)}],
        "bufferViews": buffer_views,
        "accessors": accessors,
        "animations": out_animations,
    }

    json_bytes = json.dumps(out_gltf, separators=(",", ":")).encode("utf-8")
    while len(json_bytes) % 4 != 0:
        json_bytes += b" "

    total_len = 12 + 8 + len(json_bytes) + 8 + len(out_bin)
    header = struct.pack("<III", 0x46546C67, 2, total_len)
    json_chunk = struct.pack("<II", len(json_bytes), 0x4E4F534A) + json_bytes
    bin_chunk = struct.pack("<II", len(out_bin), 0x004E4942) + bytes(out_bin)
    path.write_bytes(header + json_chunk + bin_chunk)


def main():
    root = Path(__file__).resolve().parent.parent
    paths = sorted((root / "assets" / "models" / "animated").glob("*.glb"))
    for p in paths:
        build_glb(p)
        print(f"Generated sculpted character model: {p.name} ({p.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
