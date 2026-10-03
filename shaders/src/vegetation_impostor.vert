#version 450
layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoord;
layout(location = 2) in vec4 ColorType;

layout(location = 0) out vec2 fsin_TexCoord;
layout(location = 1) out vec4 fsin_ColorType;
layout(location = 2) out float fsin_Distance;
layout(location = 3) out vec3 fsin_WorldPosition;

void main()
{
    vec4 viewPosition = View * vec4(Position, 1.0);
    gl_Position = Projection * viewPosition;

    fsin_TexCoord = TexCoord;
    fsin_ColorType = ColorType;
    fsin_Distance = length(viewPosition.xyz);
    fsin_WorldPosition = Position;
}
