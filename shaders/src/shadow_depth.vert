#version 450
layout(set = 0, binding = 0) uniform ShadowDepthBuffer
{
    mat4 LightViewProjection;
};

layout(location = 0) in vec3 Position;

void main()
{
    gl_Position = LightViewProjection * vec4(Position, 1.0);
}
