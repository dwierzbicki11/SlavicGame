#version 450
layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
};

layout(location = 0) in vec3 fsin_Color;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) in float fsin_Distance;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    vec3 normal = normalize(fsin_Normal);
    vec3 sunDirection = normalize(Lighting.yzw);
    float direct = max(dot(normal, sunDirection), 0.0);
    float daylight = max(Lighting.x, 0.02);

    vec3 color = fsin_Color * (0.30 + direct * daylight * 0.88);

    float density = max(FogColorDensity.w, 0.00001);
    float fogFactor = 1.0 - exp(-density * fsin_Distance);
    fogFactor = clamp(fogFactor, 0.0, 0.94);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    fsout_Color = vec4(color, 1.0);
}
