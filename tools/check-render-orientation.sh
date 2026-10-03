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
