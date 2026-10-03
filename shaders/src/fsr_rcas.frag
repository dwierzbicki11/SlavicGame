#version 450
#extension GL_EXT_samplerless_texture_functions : require
#extension GL_GOOGLE_include_directive : require

#define A_GPU 1
#define A_GLSL 1
#include "../vendor/amd_fsr1/ffx_a.h"

layout(set = 0, binding = 0) uniform FsrRcasConstants
{
    uvec4 Const0;
};

layout(set = 0, binding = 1) uniform texture2D EasuedColor;
layout(set = 0, binding = 2) uniform sampler EasuedSampler;

#define FSR_RCAS_F 1

AF4 FsrRcasLoadF(ASU2 p)
{
    ivec2 size = textureSize(sampler2D(EasuedColor, EasuedSampler), 0);
    ivec2 q = clamp(ivec2(p), ivec2(0), size - ivec2(1));

    // EASU renders into another offscreen texture. RCAS is the pass that
    // finally presents that texture to the swapchain, so this last
    // offscreen->swapchain transition needs one vertical correction.
    //
    // Native FSR never enters the EASU/RCAS path, so Ultra remains unchanged.
    q.y = size.y - 1 - q.y;
    return texelFetch(sampler2D(EasuedColor, EasuedSampler), q, 0);
}

void FsrRcasInputF(inout AF1 r, inout AF1 g, inout AF1 b)
{
}

#include "../vendor/amd_fsr1/ffx_fsr1.h"

layout(location = 0) out vec4 fsout_Color;

void main()
{
    AU2 pixel = AU2(gl_FragCoord.xy);
    AF3 color;
    FsrRcasF(color.r, color.g, color.b, pixel, Const0);
    fsout_Color = vec4(color, 1.0);
}
