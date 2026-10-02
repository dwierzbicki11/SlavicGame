#version 450
layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec3 PrimaryWeights;
layout(location = 3) in vec3 SecondaryWeights;

layout(location = 0) out vec3 fsin_WorldPosition;
layout(location = 1) out vec3 fsin_WorldNormal;
layout(location = 2) out vec3 fsin_PrimaryWeights;
layout(location = 3) out vec3 fsin_SecondaryWeights;
layout(location = 4) out float fsin_Distance;

void main()
{
    vec4 viewPosition = View * vec4(Position, 1.0);
    gl_Position = Projection * viewPosition;

    fsin_WorldPosition = Position;
    fsin_WorldNormal = normalize(Normal);
    fsin_PrimaryWeights = PrimaryWeights;
    fsin_SecondaryWeights = SecondaryWeights;
    fsin_Distance = length(viewPosition.xyz);
}
