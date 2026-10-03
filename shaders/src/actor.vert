#version 450
layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Color;
layout(location = 2) in vec3 Normal;

layout(location = 0) out vec3 fsin_Color;
layout(location = 1) out vec3 fsin_Normal;
layout(location = 2) out float fsin_Distance;
layout(location = 3) out vec3 fsin_WorldPosition;

void main()
{
    vec4 viewPosition = View * vec4(Position, 1.0);
    gl_Position = Projection * viewPosition;
    fsin_Color = Color;
    fsin_Normal = normalize(Normal);
    fsin_Distance = length(viewPosition.xyz);
    fsin_WorldPosition = Position;
}
