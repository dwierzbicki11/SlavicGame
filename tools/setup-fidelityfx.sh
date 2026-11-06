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
for dependency in git g++ python3 glslangValidator; do
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
if command -v cmake >/dev/null && [[ -f /usr/include/vulkan/vulkan.h ]] && [[ -e /usr/lib/x86_64-linux-gnu/libvulkan.so || -e /usr/lib64/libvulkan.so || -e /usr/lib/libvulkan.so ]]; then
    cmake -S "$ROOT_DIR/native/fidelityfx-linux" -B "$BUILD_DIR" \
        -DCMAKE_BUILD_TYPE=Release -DFFX_SDK_ROOT="$SDK_DIR" -DBUILD_TESTING=ON
    cmake --build "$BUILD_DIR" --parallel 4
    if [[ "$RUN_TESTS" == 1 ]]; then
        ctest --test-dir "$BUILD_DIR" --output-on-failure
        mkdir -p "$ROOT_DIR/tests/native/fidelityfx"
        cp "$BUILD_DIR/libslavic_fg_fixture.so" "$ROOT_DIR/tests/native/fidelityfx/libslavic_fg_fixture.so"
    fi
else
    VULKAN_INCLUDE_FLAGS=()
    if [[ ! -f /usr/include/vulkan/vulkan.h ]]; then
        VK_HEADERS_DIR="$ROOT_DIR/.cache/vulkan-headers"
        if [[ ! -f "$VK_HEADERS_DIR/include/vulkan/vulkan.h" ]]; then
            mkdir -p "$(dirname "$VK_HEADERS_DIR")"
            git clone --depth 1 https://github.com/KhronosGroup/Vulkan-Headers.git "$VK_HEADERS_DIR"
        fi
        VULKAN_INCLUDE_FLAGS=(-I"$VK_HEADERS_DIR/include")
    fi
    VULKAN_LIB=""
    for candidate in /usr/lib/x86_64-linux-gnu/libvulkan.so /usr/lib/x86_64-linux-gnu/libvulkan.so.1 /usr/lib64/libvulkan.so /usr/lib64/libvulkan.so.1 /usr/lib/libvulkan.so /usr/lib/libvulkan.so.1; do
        if [[ -e "$candidate" ]]; then
            VULKAN_LIB="$candidate"
            break
        fi
    done
    if [[ -z "$VULKAN_LIB" ]]; then
        echo "Missing libvulkan.so.1 runtime loader." >&2
        exit 127
    fi
    mkdir -p "$BUILD_DIR"
    python3 "$ROOT_DIR/native/fidelityfx-linux/prepare_sdk.py" "$SDK_DIR" "$BUILD_DIR"
    if [[ ! -f "$BUILD_DIR/fsr_shaders.cpp" || "$ROOT_DIR/native/fidelityfx-linux/compile_shaders.py" -nt "$BUILD_DIR/fsr_shaders.cpp" ]]; then
        python3 "$ROOT_DIR/native/fidelityfx-linux/compile_shaders.py" \
            --sdk "$SDK_DIR" --output "$BUILD_DIR" --compiler glslangValidator
    fi
    g++ -std=c++17 -O3 -fPIC -fvisibility=hidden -shared \
        -I"$BUILD_DIR/include" -I"$SDK_DIR/sdk/include" -I"$SDK_DIR/ffx-api/include" \
        -I"$SDK_DIR/sdk/src/backends/shared" -I"$SDK_DIR/sdk/src/shared" \
        "${VULKAN_INCLUDE_FLAGS[@]}" \
        -include "$ROOT_DIR/native/fidelityfx-linux/linux_compat.h" \
        -include FidelityFX/host/ffx_util.h \
        "$ROOT_DIR/native/fidelityfx-linux/bridge.cpp" \
        "$ROOT_DIR/native/fidelityfx-linux/frame_generation.cpp" \
        "$BUILD_DIR/fsr_shaders.cpp" \
        "$SDK_DIR/sdk/src/components/fsr3upscaler/ffx_fsr3upscaler.cpp" \
        "$SDK_DIR/sdk/src/components/fsr3/ffx_fsr3.cpp" \
        "$SDK_DIR/sdk/src/components/frameinterpolation/ffx_frameinterpolation.cpp" \
        "$SDK_DIR/sdk/src/components/opticalflow/ffx_opticalflow.cpp" \
        "$BUILD_DIR/ffx_vk_linux.cpp" \
        "$SDK_DIR/sdk/src/shared/ffx_assert.cpp" \
        "$SDK_DIR/sdk/src/shared/ffx_message.cpp" \
        "$SDK_DIR/sdk/src/shared/ffx_object_management.cpp" \
        "$SDK_DIR/sdk/src/shared/ffx_breadcrumbs_list.cpp" \
        -Wl,--no-undefined -Wl,-soname,libslavic_fsr3_vk.so \
        "$VULKAN_LIB" -o "$BUILD_DIR/libslavic_fsr3_vk.so"
    if [[ "$RUN_TESTS" == 1 ]]; then
        python3 "$ROOT_DIR/native/fidelityfx-linux/test_shader_bindings.py" --compiler glslangValidator
        g++ -std=c++17 -O3 -fPIC -fvisibility=hidden -shared \
            -I"$SDK_DIR/ffx-api/include" "${VULKAN_INCLUDE_FLAGS[@]}" \
            -include "$ROOT_DIR/native/fidelityfx-linux/linux_compat.h" \
            "$ROOT_DIR/native/fidelityfx-linux/managed_fixture.cpp" \
            -Wl,-rpath,'$ORIGIN' "$BUILD_DIR/libslavic_fsr3_vk.so" "$VULKAN_LIB" \
            -o "$BUILD_DIR/libslavic_fg_fixture.so"
        g++ -std=c++17 -O3 \
            -I"$SDK_DIR/ffx-api/include" "${VULKAN_INCLUDE_FLAGS[@]}" \
            -include "$ROOT_DIR/native/fidelityfx-linux/linux_compat.h" \
            "$ROOT_DIR/native/fidelityfx-linux/smoke.cpp" \
            -Wl,-rpath,'$ORIGIN' "$BUILD_DIR/libslavic_fsr3_vk.so" "$VULKAN_LIB" \
            -o "$BUILD_DIR/fsr3_vulkan_smoke"
        "$BUILD_DIR/fsr3_vulkan_smoke"
        mkdir -p "$ROOT_DIR/tests/native/fidelityfx"
        cp "$BUILD_DIR/libslavic_fg_fixture.so" "$ROOT_DIR/tests/native/fidelityfx/libslavic_fg_fixture.so"
    fi
fi
mkdir -p "$ROOT_DIR/native/fidelityfx"
cp "$BUILD_DIR/libslavic_fsr3_vk.so" "$ROOT_DIR/native/fidelityfx/libslavic_fsr3_vk.so"
cp "$ROOT_DIR/native/fidelityfx-linux/AMD-LICENSE.txt" "$ROOT_DIR/native/fidelityfx/AMD-LICENSE.txt"
echo "[FidelityFX] Native Linux FSR 3.1.4 + Frame Generation installed."
echo "[FidelityFX] Build/run the game, select UPSCALER -> FSR3 and FRAME GENERATION -> ON, then restart the game."
