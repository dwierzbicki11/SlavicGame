#!/usr/bin/env bash
set -euo pipefail

for file in tools/tts/setup-local.sh tools/tts/setup-local.ps1; do
  if grep -E 'pip install( --upgrade)? "chatterbox-tts|pip install chatterbox-tts' "$file" | grep -v -- '--no-deps' >/dev/null 2>&1; then
    echo "Unsafe full Chatterbox dependency resolution detected in $file"
    exit 1
  fi
done

grep -q -- '--no-deps' tools/tts/setup-local.sh
grep -q -- '--no-deps' tools/tts/setup-local.ps1
grep -q 'numpy==1.26.4' tools/tts/setup-local.sh

echo "Local TTS installer policy: minimal Python 3.12 runtime."
