#version 450
#extension GL_EXT_samplerless_texture_functions : require
#extension GL_GOOGLE_include_directive : require

#define A_GPU 1
#define A_GLSL 1
#include "../vendor/amd_fsr1/ffx_a.h"

layout(set = 0, binding = 0) uniform FsrEasuConstants
{
    uvec4 Const0;
    uvec4 Const1;
    uvec4 Const2;
    uvec4 Const3;
};

layout(set = 0, binding = 1) uniform texture2D SceneColor;
layout(set = 0, binding = 2) uniform sampler SceneSampler;

#define FSR_EASU_F 1

AF4 FsrEasuRF(AF2 p)
{
    return textureGather(sampler2D(SceneColor, SceneSampler), p, 0);
}

AF4 FsrEasuGF(AF2 p)
{
    return textureGather(sampler2D(SceneColor, SceneSampler), p, 1);
}

AF4 FsrEasuBF(AF2 p)
{
    return textureGather(sampler2D(SceneColor, SceneSampler), p, 2);
}

#include "../vendor/amd_fsr1/ffx_fsr1.h"

layout(location = 0) out vec4 fsout_Color;

void main()
{
    AU2 pixel = AU2(gl_FragCoord.xy);
    AF3 color;
    FsrEasuF(color, pixel, Const0, Const1, Const2, Const3);
    fsout_Color = vec4(color, 1.0);
}
