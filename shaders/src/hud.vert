#version 450
layout(set = 0, binding = 0) uniform ScreenBuffer { vec4 ScreenSize; };

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 0) out vec4 fsin_Color;

void main()
{
    vec2 ndc = vec2(
        Position.x / ScreenSize.x * 2.0 - 1.0,
        1.0 - Position.y / ScreenSize.y * 2.0);
    gl_Position = vec4(ndc, 0.0, 1.0);
    fsin_Color = Color;
}
