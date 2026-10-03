#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
VENV="$ROOT/.venv-tts"

PYTHON="${PYTHON:-python3}"
"$PYTHON" -c 'import sys; assert sys.version_info[:2] == (3,11), "Chatterbox setup currently expects Python 3.11"'

"$PYTHON" -m venv "$VENV"
"$VENV/bin/python" -m pip install --upgrade pip
"$VENV/bin/python" -m pip install chatterbox-tts

echo
echo "Local Chatterbox TTS installed."
echo "The first spoken line will download the open model weights; later use can work from the local cache."
echo "Optional: put legally usable WAV references in assets/voice/reference/ for per-character/emotion voices."
