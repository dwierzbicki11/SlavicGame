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

compile_shader() {
    local source="$1"
    local output="$2"
    local stage="$3"

    echo "[shader] $source -> $output"

    if [[ "$COMPILER" == "glslc" ]]; then
        glslc             --target-env=vulkan1.2             -O             "$source"             -o "$output"
    else
        glslangValidator             -V             --target-env vulkan1.2             -S "$stage"             "$source"             -o "$output"
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
compile_shader "$SRC_DIR/fsr_easu.vert"     "$OUT_DIR/fsr_easu.vert.spv"     vert
compile_shader "$SRC_DIR/fsr_easu.frag"     "$OUT_DIR/fsr_easu.frag.spv"     frag
compile_shader "$SRC_DIR/fsr_rcas.vert"     "$OUT_DIR/fsr_rcas.vert.spv"     vert
compile_shader "$SRC_DIR/fsr_rcas.frag"     "$OUT_DIR/fsr_rcas.frag.spv"     frag
compile_shader "$SRC_DIR/vegetation_impostor.vert" "$OUT_DIR/vegetation_impostor.vert.spv" vert
compile_shader "$SRC_DIR/vegetation_impostor.frag" "$OUT_DIR/vegetation_impostor.frag.spv" frag

echo "[shader] Gotowe. SPIR-V: $OUT_DIR"
