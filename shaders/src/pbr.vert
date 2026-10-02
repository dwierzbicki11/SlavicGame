#version 450
layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;

layout(location = 0) out vec3 fsin_WorldPosition;
layout(location = 1) out vec3 fsin_WorldNormal;
layout(location = 2) out vec2 fsin_TexCoord;
layout(location = 3) out float fsin_Distance;
layout(location = 4) out vec3 fsin_CameraPosition;

void main()
{
    vec4 viewPosition = View * vec4(Position, 1.0);
    gl_Position = Projection * viewPosition;

    fsin_WorldPosition = Position;
    fsin_WorldNormal = normalize(Normal);
    fsin_TexCoord = TexCoord;
    fsin_Distance = length(viewPosition.xyz);
    fsin_CameraPosition = inverse(View)[3].xyz;
}
