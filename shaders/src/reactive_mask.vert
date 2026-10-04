#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Color;
layout(location = 2) in vec3 Normal;

void main()
{
    gl_Position = Projection * View * vec4(Position, 1.0);
}
