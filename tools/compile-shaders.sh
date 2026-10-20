#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SRC_DIR="$ROOT_DIR/shaders/src"
OUT_DIR="$ROOT_DIR/shaders/bin"

mkdir -p "$OUT_DIR"

if command -v glslc >/dev/null 2>&1; then
    COMPILER="glslc"
elif command -v glslangValidator >/dev/null 2>&1; then
    COMPILER="glslangValidator"
else
    cat >&2 <<'EOF'
BŁĄD: nie znaleziono kompilatora shaderów GLSL -> SPIR-V.

Zainstaluj jeden z:
  sudo apt install glslc
albo:
  sudo apt install glslang-tools

Potem uruchom ponownie:
  ./tools/compile-shaders.sh
EOF
    exit 127
fi

# Build every SPIR-V in a temporary directory before publishing any result.
# Failed GLSL compilation must not leave a mix of old and new shader binaries.
STAGING_DIR="$(mktemp -d "$OUT_DIR/.compile-shaders.XXXXXXXX")"
trap 'rm -rf -- "$STAGING_DIR"' EXIT

compile_shader() {
    local source="$1"
    local output="$STAGING_DIR/${2##*/}"
    local stage="$3"

    echo "[shader] $source -> $output"

    # Scene shaders also run on the Vulkan 1.0 fallback device. Native optical
    # flow is compiled separately for 1.1 and gated by creation capabilities.
    if [[ "$COMPILER" == "glslc" ]]; then
        glslc             --target-env=vulkan1.0             -O             "$source"             -o "$output"
    else
        glslangValidator             -V             --target-env vulkan1.0             -S "$stage"             "$source"             -o "$output"
    fi
}

compile_shader "$SRC_DIR/terrain.vert" "$OUT_DIR/terrain.vert.spv" vert
compile_shader "$SRC_DIR/terrain.frag" "$OUT_DIR/terrain.frag.spv" frag
compile_shader "$SRC_DIR/actor.vert"   "$OUT_DIR/actor.vert.spv"   vert
compile_shader "$SRC_DIR/actor.frag"   "$OUT_DIR/actor.frag.spv"   frag
compile_shader "$SRC_DIR/hud.vert"     "$OUT_DIR/hud.vert.spv"     vert
compile_shader "$SRC_DIR/hud.frag"     "$OUT_DIR/hud.frag.spv"     frag
compile_shader "$SRC_DIR/pbr.vert"     "$OUT_DIR/pbr.vert.spv"     vert
compile_shader "$SRC_DIR/pbr.frag"     "$OUT_DIR/pbr.frag.spv"     frag
compile_shader "$SRC_DIR/sky.vert"          "$OUT_DIR/sky.vert.spv"          vert
compile_shader "$SRC_DIR/sky.frag"          "$OUT_DIR/sky.frag.spv"          frag
compile_shader "$SRC_DIR/shadow_depth.vert" "$OUT_DIR/shadow_depth.vert.spv" vert
compile_shader "$SRC_DIR/shadow_depth.frag" "$OUT_DIR/shadow_depth.frag.spv" frag
compile_shader "$SRC_DIR/present.vert"      "$OUT_DIR/present.vert.spv"      vert
compile_shader "$SRC_DIR/present.frag"      "$OUT_DIR/present.frag.spv"      frag
compile_shader "$SRC_DIR/motion_vectors.vert" "$OUT_DIR/motion_vectors.vert.spv" vert
compile_shader "$SRC_DIR/motion_vectors.frag" "$OUT_DIR/motion_vectors.frag.spv" frag
compile_shader "$SRC_DIR/motion_dynamic.vert" "$OUT_DIR/motion_dynamic.vert.spv" vert
compile_shader "$SRC_DIR/motion_dynamic.frag" "$OUT_DIR/motion_dynamic.frag.spv" frag
compile_shader "$SRC_DIR/reactive_mask.vert" "$OUT_DIR/reactive_mask.vert.spv" vert
compile_shader "$SRC_DIR/reactive_mask.frag" "$OUT_DIR/reactive_mask.frag.spv" frag
compile_shader "$SRC_DIR/postprocess.vert"  "$OUT_DIR/postprocess.vert.spv"  vert
compile_shader "$SRC_DIR/postprocess.frag"  "$OUT_DIR/postprocess.frag.spv"  frag
compile_shader "$SRC_DIR/bloom.vert"        "$OUT_DIR/bloom.vert.spv"        vert
compile_shader "$SRC_DIR/bloom.frag"        "$OUT_DIR/bloom.frag.spv"        frag
compile_shader "$SRC_DIR/fsr_easu.vert"     "$OUT_DIR/fsr_easu.vert.spv"     vert
compile_shader "$SRC_DIR/fsr_easu.frag"     "$OUT_DIR/fsr_easu.frag.spv"     frag
compile_shader "$SRC_DIR/fsr_rcas.vert"     "$OUT_DIR/fsr_rcas.vert.spv"     vert
compile_shader "$SRC_DIR/fsr_rcas.frag"     "$OUT_DIR/fsr_rcas.frag.spv"     frag
compile_shader "$SRC_DIR/vegetation_impostor.vert" "$OUT_DIR/vegetation_impostor.vert.spv" vert
compile_shader "$SRC_DIR/vegetation_impostor.frag" "$OUT_DIR/vegetation_impostor.frag.spv" frag

# Back up published binaries before replacement. Roll back on handled
# publication errors; this is not a crash-atomic multi-file transaction.
BACKUP_DIR="$STAGING_DIR/.previous"
mkdir -p "$BACKUP_DIR"
for staged in "$STAGING_DIR"/*.spv; do
    target="$OUT_DIR/${staged##*/}"
    if [[ -f "$target" ]]; then
        cp -p -- "$target" "$BACKUP_DIR/"
    fi
done

published=()
rollback_publish() {
    local target previous
    for target in "${published[@]}"; do
        previous="$BACKUP_DIR/${target##*/}"
        if [[ -f "$previous" ]]; then
            cp -p -- "$previous" "$target"
        else
            rm -f -- "$target"
        fi
    done
}
for staged in "$STAGING_DIR"/*.spv; do
    target="$OUT_DIR/${staged##*/}"
    if ! mv -f -- "$staged" "$target"; then
        rollback_publish
        echo "[shader] Failed to publish complete SPIR-V set; previous outputs restored" >&2
        exit 1
    fi
    published+=("$target")
done

echo "[shader] Gotowe. SPIR-V: $OUT_DIR"
