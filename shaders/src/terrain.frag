#version 450
layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
};

layout(location = 0) in vec3 fsin_Color;
layout(location = 1) in float fsin_Distance;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    vec3 litColor = fsin_Color * Lighting.x;
    float fogFactor = 1.0 - exp(-FogColorDensity.w * fsin_Distance);
    fogFactor = clamp(fogFactor, 0.0, 0.94);
    vec3 color = mix(litColor, FogColorDensity.rgb, fogFactor);
    fsout_Color = vec4(color, 1.0);
}
