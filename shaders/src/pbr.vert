#version 450
layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;

layout(location = 0) out vec3 fsin_ViewPosition;
layout(location = 1) out vec3 fsin_ViewNormal;
layout(location = 2) out vec2 fsin_TexCoord;
layout(location = 3) out float fsin_Distance;

void main()
{
    vec4 viewPosition = View * vec4(Position, 1.0);
    gl_Position = Projection * viewPosition;
    fsin_ViewPosition = viewPosition.xyz;
    fsin_ViewNormal = normalize(mat3(View) * Normal);
    fsin_TexCoord = TexCoord;
    fsin_Distance = length(viewPosition.xyz);
}
