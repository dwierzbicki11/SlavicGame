#version 450
#extension GL_GOOGLE_include_directive : require
#include "../include/clouds.glsl"

layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
    vec4 SunColorTime;
    vec4 SkyWeather;
};

layout(location = 0) in vec3 fsin_Color;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) in float fsin_Distance;
layout(location = 3) in vec3 fsin_WorldPosition;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    vec3 normal = normalize(fsin_Normal);
    vec3 sunDirection = normalize(Lighting.yzw);
    float direct = max(dot(normal, sunDirection), 0.0);
    float daylight = max(Lighting.x, 0.02);

    vec3 sunColor = SunColorTime.rgb;
    float cloudShadow = CloudShadowFactor(
        fsin_WorldPosition,
        sunDirection,
        SkyWeather.w,
        SkyWeather.z,
        SkyWeather.x);
    vec3 ambient = fsin_Color * 0.26;
    vec3 color = ambient +
        fsin_Color * sunColor * direct * daylight * cloudShadow * 0.92;

    float density = max(FogColorDensity.w, 0.00001);
    float fogFactor = 1.0 - exp(-density * fsin_Distance);
    fogFactor = clamp(fogFactor, 0.0, 0.94);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    fsout_Color = vec4(color, 1.0);
}
