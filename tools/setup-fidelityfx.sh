#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SDK_DIR="${SLAVICGAME_FFX_SDK_DIR:-$ROOT_DIR/.cache/fidelityfx-sdk-v1.1.4}"
BUILD_DIR="${SLAVICGAME_FFX_BUILD_DIR:-$ROOT_DIR/.cache/fidelityfx-linux-build}"
REPO="https://github.com/GPUOpen-LibrariesAndSDKs/FidelityFX-SDK.git"
SDK_SHA="c6efa6bf7f2027b3ec94f28578bb5965eabb9e55"
RUN_TESTS=0
if [[ "${1:-}" == "--test" && "$#" -eq 1 ]]; then
    RUN_TESTS=1
elif [[ "$#" -ne 0 ]]; then
    echo "Usage: $0 [--test]" >&2
    exit 2
fi
if [[ "$(uname -s)" != Linux ]]; then
    echo "This helper builds the native Linux provider; use setup-fidelityfx.ps1 on Windows." >&2
    exit 2
fi
for dependency in git cmake g++ python3 glslangValidator; do
    if ! command -v "$dependency" >/dev/null; then
        echo "Missing $dependency. On Debian/Ubuntu/Mint: sudo apt install git cmake g++ python3 libvulkan-dev glslang-tools" >&2
        exit 127
    fi
done
if [[ ! -d "$SDK_DIR/.git" ]]; then
    if [[ -e "$SDK_DIR" ]]; then
        echo "SDK path exists but is not a git checkout: $SDK_DIR" >&2
        exit 1
    fi
    mkdir -p "$(dirname "$SDK_DIR")"
    git clone --depth 1 --filter=blob:none --sparse --branch v1.1.4 "$REPO" "$SDK_DIR"
    git -C "$SDK_DIR" sparse-checkout set sdk ffx-api
fi
if [[ "$(git -C "$SDK_DIR" rev-parse HEAD)" != "$SDK_SHA" || -n "$(git -C "$SDK_DIR" status --porcelain --untracked-files=no)" ]]; then
    echo "SDK checkout must be unmodified v1.1.4 ($SDK_SHA): $SDK_DIR" >&2
    exit 1
fi
cmake -S "$ROOT_DIR/native/fidelityfx-linux" -B "$BUILD_DIR" \
    -DCMAKE_BUILD_TYPE=Release -DFFX_SDK_ROOT="$SDK_DIR" -DBUILD_TESTING=ON
cmake --build "$BUILD_DIR" --parallel 4
if [[ "$RUN_TESTS" == 1 ]]; then
    ctest --test-dir "$BUILD_DIR" --output-on-failure
fi
mkdir -p "$ROOT_DIR/native/fidelityfx"
cp "$BUILD_DIR/libslavic_fsr3_vk.so" "$ROOT_DIR/native/fidelityfx/libslavic_fsr3_vk.so"
cp "$ROOT_DIR/native/fidelityfx-linux/AMD-LICENSE.txt" "$ROOT_DIR/native/fidelityfx/AMD-LICENSE.txt"
echo "[FidelityFX] Native Linux FSR 3.1.4 installed. Build the game again, then select UPSCALER -> FSR3 in the graphics menu."
