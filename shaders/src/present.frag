#version 450

layout(set = 0, binding = 0) uniform texture2D SceneColor;
layout(set = 0, binding = 1) uniform sampler SceneSampler;
layout(set = 0, binding = 2) uniform PresentationParameters
{
    vec4 Params;
};

layout(location = 0) in vec2 fsin_TexCoord;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    float flipY = Params.x;
    vec2 presentUv = vec2(
        fsin_TexCoord.x,
        mix(fsin_TexCoord.y, 1.0 - fsin_TexCoord.y, flipY));

    fsout_Color = texture(
        sampler2D(SceneColor, SceneSampler),
        presentUv);
}
