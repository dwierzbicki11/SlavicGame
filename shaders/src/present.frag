#version 450

layout(set = 0, binding = 0) uniform texture2D SceneColor;
layout(set = 0, binding = 1) uniform sampler SceneSampler;

layout(location = 0) in vec2 fsin_TexCoord;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    // Offscreen Vulkan framebuffer textures are vertically inverted relative
    // to the presentation pass coordinates used by our fullscreen triangle.
    // Flip only the sampled Y coordinate; do not touch scene projection math.
    vec2 presentUv = vec2(
        fsin_TexCoord.x,
        1.0 - fsin_TexCoord.y);

    fsout_Color = texture(
        sampler2D(SceneColor, SceneSampler),
        presentUv);
}
