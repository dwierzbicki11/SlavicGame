#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CACHE_DIR="$ROOT_DIR/.cache/fidelityfx-sdk-v1.1.4"
REPO="https://github.com/GPUOpen-LibrariesAndSDKs/FidelityFX-SDK.git"

cat <<'EOF'
[FidelityFX] AMD FidelityFX SDK 1.1.4 exposes Vulkan, but AMD's official
prebuilt FSR 3.1.4 runtime is a Windows DLL. SlavicGame therefore does not
pretend that copying that DLL produces a native Linux implementation.

This helper fetches the exact open-source SDK revision so the Linux Vulkan
provider can be built from source by the SlavicGame native bridge.
EOF

mkdir -p "$(dirname "$CACHE_DIR")"
if [[ -d "$CACHE_DIR/.git" ]]; then
    git -C "$CACHE_DIR" fetch --tags --depth 1 origin v1.1.4
    git -C "$CACHE_DIR" checkout --force v1.1.4
else
    rm -rf "$CACHE_DIR"
    git clone --depth 1 --branch v1.1.4 "$REPO" "$CACHE_DIR"
fi

echo "[FidelityFX] SDK source ready: $CACHE_DIR"
echo "[FidelityFX] No unsupported Windows DLL has been installed on Linux."
