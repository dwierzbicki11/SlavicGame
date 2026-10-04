#version 450

layout(set = 1, binding = 0) uniform ReactiveParameters
{
    vec4 Parameters;
};

layout(location = 0) out float Reactive;

void main()
{
    Reactive = clamp(Parameters.x, 0.0, 1.0);
}
