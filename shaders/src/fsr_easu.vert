#version 450

// Only the intermediate EASU target must preserve texture row order.
// Final presentation keeps the existing scene-to-display convention.
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
