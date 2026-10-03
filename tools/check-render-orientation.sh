#!/usr/bin/env bash
set -euo pipefail

files=(
  shaders/src/present.frag
  shaders/src/fsr_easu.frag
  shaders/src/fsr_rcas.frag
)

for file in "${files[@]}"; do
  if grep -Eq 'flipY|PresentationParameters|1\.0[[:space:]]*-[[:space:]]*fsin_TexCoord\.y|size\.y[[:space:]]*-[[:space:]]*1[[:space:]]*-[[:space:]]*q\.y|mix\(p\.y[^\n]*1\.0[^\n]*p\.y' "$file"; then
    echo "Manual Vulkan Y-flip detected in $file"
    exit 1
  fi
done

echo "Fullscreen Vulkan orientation check: canonical UV only."


for file in shaders/src/fsr_easu.frag shaders/src/fsr_rcas.frag; do
  if grep -q 'gl_FragCoord' "$file"; then
    echo "FSR pixel coordinates must come from canonical fullscreen UVs, not gl_FragCoord: $file"
    exit 1
  fi
  if ! grep -q 'fsin_TexCoord' "$file"; then
    echo "FSR shader is missing canonical fullscreen UV input: $file"
    exit 1
  fi
done

echo "FSR pixel-coordinate check: UV-derived coordinates only."
