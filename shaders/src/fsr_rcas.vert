#version 450

// RCAS can render either to the OS swapchain or to FG's offscreen color.
// The latter must retain texture row order on Vulkan.
layout(constant_id = 0) const uint PreserveTextureRows = 0u;
layout(location = 0) out vec2 fsin_TexCoord;

void main()
{
    vec2 uv = vec2(
        float((gl_VertexIndex << 1) & 2),
        float(gl_VertexIndex & 2));

    fsin_TexCoord = uv;
    vec2 position = uv * 2.0 - 1.0;
    if (PreserveTextureRows != 0u) position.y = -position.y;
    gl_Position = vec4(
        position,
        0.0,
        1.0);
}
