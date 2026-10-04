cmake_minimum_required(VERSION 3.21)
project(SlavicFsr3Linux LANGUAGES CXX)
if(NOT CMAKE_SYSTEM_NAME STREQUAL "Linux")
    message(FATAL_ERROR "This source-built provider targets native Linux only")
endif()
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
find_package(Vulkan REQUIRED)
find_package(Python3 REQUIRED COMPONENTS Interpreter)
find_program(GLSLANG_VALIDATOR glslangValidator REQUIRED)
set(FFX_SDK_ROOT "" CACHE PATH "FidelityFX SDK v1.1.4 source directory")
if(NOT EXISTS "${FFX_SDK_ROOT}/sdk/src/components/fsr3upscaler/ffx_fsr3upscaler.cpp")
    message(FATAL_ERROR "Set FFX_SDK_ROOT to the pinned SDK source directory")
endif()
# The opaque SDK allocation assumes Windows' 16-bit wchar_t. All Linux SDK
# structs use native 32-bit wchar_t, so reserve more space without changing the
# pointer-only API boundary or compiling against an incompatible libc ABI.
execute_process(COMMAND "${Python3_EXECUTABLE}" "${CMAKE_CURRENT_SOURCE_DIR}/prepare_sdk.py"
    "${FFX_SDK_ROOT}" "${CMAKE_CURRENT_BINARY_DIR}"
    COMMAND_ERROR_IS_FATAL ANY)
set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS prepare_sdk.py
    "${FFX_SDK_ROOT}/sdk/src/backends/vk/ffx_vk.cpp")
file(GLOB_RECURSE FSR_SHADER_DEPS CONFIGURE_DEPENDS
    "${FFX_SDK_ROOT}/sdk/include/FidelityFX/gpu/*.h"
    "${FFX_SDK_ROOT}/sdk/src/backends/vk/shaders/fsr3upscaler/*.glsl")
add_custom_command(OUTPUT "${CMAKE_CURRENT_BINARY_DIR}/fsr_shaders.cpp"
    COMMAND Python3::Interpreter "${CMAKE_CURRENT_SOURCE_DIR}/compile_shaders.py"
        --sdk "${FFX_SDK_ROOT}" --output "${CMAKE_CURRENT_BINARY_DIR}"
        --compiler "${GLSLANG_VALIDATOR}"
    DEPENDS ${FSR_SHADER_DEPS} compile_shaders.py VERBATIM)
add_library(slavic_fsr3_vk SHARED bridge.cpp "${CMAKE_CURRENT_BINARY_DIR}/fsr_shaders.cpp"
    "${FFX_SDK_ROOT}/sdk/src/components/fsr3upscaler/ffx_fsr3upscaler.cpp"
    "${FFX_SDK_ROOT}/sdk/src/components/fsr3/ffx_fsr3.cpp"
    "${FFX_SDK_ROOT}/sdk/src/components/frameinterpolation/ffx_frameinterpolation.cpp"
    "${FFX_SDK_ROOT}/sdk/src/components/opticalflow/ffx_opticalflow.cpp"
    "${CMAKE_CURRENT_BINARY_DIR}/ffx_vk_linux.cpp"
    "${FFX_SDK_ROOT}/sdk/src/shared/ffx_assert.cpp"
    "${FFX_SDK_ROOT}/sdk/src/shared/ffx_message.cpp"
    "${FFX_SDK_ROOT}/sdk/src/shared/ffx_object_management.cpp"
    "${FFX_SDK_ROOT}/sdk/src/shared/ffx_breadcrumbs_list.cpp")
target_include_directories(slavic_fsr3_vk PRIVATE
    "${CMAKE_CURRENT_BINARY_DIR}/include"
    "${FFX_SDK_ROOT}/sdk/include" "${FFX_SDK_ROOT}/ffx-api/include"
    "${FFX_SDK_ROOT}/sdk/src/backends/shared" "${FFX_SDK_ROOT}/sdk/src/shared")
target_compile_options(slavic_fsr3_vk PRIVATE -include "${CMAKE_CURRENT_SOURCE_DIR}/linux_compat.h")
target_compile_options(slavic_fsr3_vk PRIVATE "SHELL:-include FidelityFX/host/ffx_util.h")
target_link_libraries(slavic_fsr3_vk PRIVATE Vulkan::Vulkan)
target_link_options(slavic_fsr3_vk PRIVATE -Wl,--no-undefined)
set_target_properties(slavic_fsr3_vk PROPERTIES CXX_VISIBILITY_PRESET hidden)
include(CTest)
if(BUILD_TESTING)
    add_executable(fsr3_vulkan_smoke smoke.cpp)
    target_include_directories(fsr3_vulkan_smoke PRIVATE "${FFX_SDK_ROOT}/ffx-api/include")
    target_compile_options(fsr3_vulkan_smoke PRIVATE -include "${CMAKE_CURRENT_SOURCE_DIR}/linux_compat.h")
    target_link_libraries(fsr3_vulkan_smoke PRIVATE slavic_fsr3_vk Vulkan::Vulkan)
    add_test(NAME fsr3_vulkan_dispatch COMMAND fsr3_vulkan_smoke)
    set_tests_properties(fsr3_vulkan_dispatch PROPERTIES TIMEOUT 120)
endif()
