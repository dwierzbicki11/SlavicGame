#version 450

layout(location = 0) noperspective in vec2 fsin_Motion;
layout(location = 0) out vec2 fsout_Motion;

void main()
{
    fsout_Motion = fsin_Motion;
}
