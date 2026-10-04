#version 450
#extension GL_GOOGLE_include_directive : require
#include "../include/clouds.glsl"
#include "../include/shadows.glsl"

layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
    vec4 SunColorTime;
    vec4 SkyWeather;
    vec4 MoonParameters;
    vec4 CelestialParameters;
    vec4 GraphicsFeatures0;
    vec4 GraphicsFeatures1;
    vec4 GraphicsFeatures2;
};

layout(set = 1, binding = 0) uniform ShadowDataBuffer
{
    mat4 LightViewProjection;
};
layout(set = 1, binding = 1) uniform texture2D ShadowMap;
layout(set = 1, binding = 2) uniform sampler ShadowSampler;

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
    float cloudShadow = 1.0;
    if (GraphicsFeatures0.y > 0.5)
    {
        cloudShadow = CloudShadowFactor(
            fsin_WorldPosition,
            sunDirection,
            SkyWeather.w,
            SkyWeather.z,
            SkyWeather.x);
    }
    float geometryShadow = 1.0;
    if (GraphicsFeatures0.z > 0.5)
    {
        geometryShadow = SampleSunShadow(
            ShadowMap,
            ShadowSampler,
            LightViewProjection,
            fsin_WorldPosition,
            normal,
            sunDirection);
    }
    float directShadow = cloudShadow * geometryShadow;
    float nightFactor = clamp(CelestialParameters.x, 0.0, 1.0);

    // Values above 1.0 are reserved for lightweight emissive actor/effect geometry.
    // Campfire flames use this path so they remain visible at night without a
    // dedicated point-light or particle render pass.
    vec3 baseColor = min(fsin_Color, vec3(1.0));
    vec3 emissive = max(fsin_Color - vec3(1.0), vec3(0.0));
    vec3 ambient = baseColor * mix(0.26, 0.045, nightFactor);
    vec3 color = ambient +
        baseColor * sunColor * direct * daylight * directShadow * 0.92 +
        emissive * 0.95;

    float density = max(FogColorDensity.w * GraphicsFeatures0.w, 0.00001);
    float fogFactor = 1.0 - exp(-density * fsin_Distance);
    fogFactor = clamp(fogFactor, 0.0, 0.94);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    fsout_Color = vec4(color, 1.0);
}
