#version 450
#extension GL_GOOGLE_include_directive : require
#include "../include/clouds.glsl"
#include "../include/shadows.glsl"

const float PI = 3.14159265359;

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
    vec4 GraphicsFeatures3;
};

layout(set = 1, binding = 0) uniform texture2D GrassBase;
layout(set = 1, binding = 1) uniform texture2D GrassNormal;
layout(set = 1, binding = 2) uniform texture2D GrassRoughness;
layout(set = 1, binding = 3) uniform texture2D GrassAo;
layout(set = 1, binding = 4) uniform texture2D GrassHeight;

layout(set = 1, binding = 5) uniform texture2D PathBase;
layout(set = 1, binding = 6) uniform texture2D PathNormal;
layout(set = 1, binding = 7) uniform texture2D PathRoughness;
layout(set = 1, binding = 8) uniform texture2D PathAo;
layout(set = 1, binding = 9) uniform texture2D PathHeight;

layout(set = 1, binding = 10) uniform texture2D LitterBase;
layout(set = 1, binding = 11) uniform texture2D LitterNormal;
layout(set = 1, binding = 12) uniform texture2D LitterRoughness;
layout(set = 1, binding = 13) uniform texture2D LitterAo;
layout(set = 1, binding = 14) uniform texture2D LitterHeight;

layout(set = 1, binding = 15) uniform texture2D MudBase;
layout(set = 1, binding = 16) uniform texture2D MudNormal;
layout(set = 1, binding = 17) uniform texture2D MudRoughness;
layout(set = 1, binding = 18) uniform texture2D MudAo;
layout(set = 1, binding = 19) uniform texture2D MudHeight;

layout(set = 1, binding = 20) uniform texture2D SwampBase;
layout(set = 1, binding = 21) uniform texture2D SwampNormal;
layout(set = 1, binding = 22) uniform texture2D SwampRoughness;
layout(set = 1, binding = 23) uniform texture2D SwampAo;
layout(set = 1, binding = 24) uniform texture2D SwampHeight;

layout(set = 1, binding = 25) uniform texture2D RockBase;
layout(set = 1, binding = 26) uniform texture2D RockNormal;
layout(set = 1, binding = 27) uniform texture2D RockRoughness;
layout(set = 1, binding = 28) uniform texture2D RockAo;
layout(set = 1, binding = 29) uniform texture2D RockHeight;

layout(set = 1, binding = 30) uniform sampler TerrainSampler;

layout(set = 2, binding = 0) uniform ShadowDataBuffer
{
    mat4 LightViewProjection;
};
layout(set = 2, binding = 1) uniform texture2D ShadowMap;
layout(set = 2, binding = 2) uniform sampler ShadowSampler;

layout(location = 0) in vec3 fsin_WorldPosition;
layout(location = 1) in vec3 fsin_WorldNormal;
layout(location = 2) in vec3 fsin_PrimaryWeights;
layout(location = 3) in vec3 fsin_SecondaryWeights;
layout(location = 4) in float fsin_Distance;

layout(location = 0) out vec4 fsout_Color;

mat2 Rotation(float angle)
{
    float c = cos(angle);
    float s = sin(angle);
    return mat2(c, -s, s, c);
}

float MacroNoise(vec2 p)
{
    float a = sin(p.x * 0.041 + p.y * 0.027);
    float b = cos(p.x * 0.019 - p.y * 0.053);
    float c = sin((p.x + p.y) * 0.013);
    return clamp(0.5 + a * 0.22 + b * 0.18 + c * 0.10, 0.0, 1.0);
}

vec2 WarpedUv(vec2 worldXZ, float metersPerTile, float seed)
{
    vec2 warp = vec2(
        sin(worldXZ.y * 0.021 + seed * 3.17),
        cos(worldXZ.x * 0.024 - seed * 2.31)) * 0.22;
    return (worldXZ + warp) / metersPerTile;
}

vec2 SecondaryUv(vec2 worldXZ, float metersPerTile, float seed)
{
    return Rotation(0.67 + seed * 0.11) *
        WarpedUv(worldXZ + vec2(17.0, -11.0), metersPerTile * 1.73, seed + 1.0);
}

float BlendMask(vec2 worldXZ, float seed)
{
    return smoothstep(0.22, 0.78, MacroNoise(worldXZ + seed * 29.0));
}

vec3 SampleBaseFast(texture2D tex, vec2 worldXZ, float metersPerTile, float seed)
{
    vec2 uv = WarpedUv(worldXZ, metersPerTile, seed);
    return texture(sampler2D(tex, TerrainSampler), uv).rgb;
}

vec3 SampleBase(texture2D tex, vec2 worldXZ, float metersPerTile, float seed)
{
    vec2 uvA = WarpedUv(worldXZ, metersPerTile, seed);
    vec2 uvB = SecondaryUv(worldXZ, metersPerTile, seed);
    float blend = BlendMask(worldXZ, seed);
    return mix(
        texture(sampler2D(tex, TerrainSampler), uvA).rgb,
        texture(sampler2D(tex, TerrainSampler), uvB).rgb,
        blend);
}

float SampleScalar(texture2D tex, vec2 worldXZ, float metersPerTile, float seed)
{
    vec2 uvA = WarpedUv(worldXZ, metersPerTile, seed);
    vec2 uvB = SecondaryUv(worldXZ, metersPerTile, seed);
    float blend = BlendMask(worldXZ, seed);
    return mix(
        texture(sampler2D(tex, TerrainSampler), uvA).r,
        texture(sampler2D(tex, TerrainSampler), uvB).r,
        blend);
}

vec3 SampleNormal(texture2D tex, vec2 worldXZ, float metersPerTile, float seed)
{
    vec2 uvA = WarpedUv(worldXZ, metersPerTile, seed);
    vec2 uvB = SecondaryUv(worldXZ, metersPerTile, seed);
    float blend = BlendMask(worldXZ, seed);

    vec3 a = texture(sampler2D(tex, TerrainSampler), uvA).xyz * 2.0 - 1.0;
    vec3 b = texture(sampler2D(tex, TerrainSampler), uvB).xyz * 2.0 - 1.0;

    a.xy *= 0.82;
    b.xy *= 0.82;
    return normalize(mix(a, b, blend));
}

mat3 GroundTangentFrame(vec3 n)
{
    vec3 tangent = normalize(vec3(1.0, -n.x / max(abs(n.y), 0.12), 0.0));
    vec3 bitangent = normalize(cross(n, tangent));
    return mat3(tangent, bitangent, n);
}


void main()
{
    vec3 weightsA = max(fsin_PrimaryWeights, vec3(0.0));
    vec3 weightsB = max(fsin_SecondaryWeights, vec3(0.0));
    vec2 worldXZ = fsin_WorldPosition.xz;

    const float grassScale = 2.1;
    const float litterScale = 2.0;
    const float pathScale = 2.6;
    const float mudScale = 2.4;
    const float swampScale = 2.8;
    const float rockScale = 2.7;

    float terrainDetail = GraphicsFeatures3.z;
    float normalMapping = GraphicsFeatures3.x;
    float specularEnabled = GraphicsFeatures3.y;

    float organic = MacroNoise(worldXZ);
    weightsA.x *= mix(0.93, 1.07, organic);
    weightsA.y *= mix(1.06, 0.94, organic);
    weightsB.x *= mix(0.92, 1.10, organic);
    weightsB.y *= mix(1.08, 0.94, organic);

    // Height-assisted blending is one of the most expensive terrain stages,
    // so Low/Medium skip it entirely.
    if (terrainDetail >= 2.0)
    {
        weightsA.x *= mix(0.62, 1.38, SampleScalar(GrassHeight, worldXZ, grassScale, 0.2));
        weightsA.y *= mix(0.62, 1.38, SampleScalar(LitterHeight, worldXZ, litterScale, 1.1));
        weightsA.z *= mix(0.62, 1.38, SampleScalar(PathHeight, worldXZ, pathScale, 2.3));
        weightsB.x *= mix(0.62, 1.38, SampleScalar(MudHeight, worldXZ, mudScale, 3.2));
        weightsB.y *= mix(0.62, 1.38, SampleScalar(SwampHeight, worldXZ, swampScale, 4.1));
        weightsB.z *= mix(0.62, 1.38, SampleScalar(RockHeight, worldXZ, rockScale, 5.4));
    }

    float weightSum =
        weightsA.x + weightsA.y + weightsA.z +
        weightsB.x + weightsB.y + weightsB.z;
    weightSum = max(weightSum, 0.0001);
    weightsA /= weightSum;
    weightsB /= weightSum;

    vec3 albedo;
    if (terrainDetail < 1.0)
    {
        albedo =
            SampleBaseFast(GrassBase, worldXZ, grassScale, 0.2) * weightsA.x +
            SampleBaseFast(LitterBase, worldXZ, litterScale, 1.1) * weightsA.y +
            SampleBaseFast(PathBase, worldXZ, pathScale, 2.3) * weightsA.z +
            SampleBaseFast(MudBase, worldXZ, mudScale, 3.2) * weightsB.x +
            SampleBaseFast(SwampBase, worldXZ, swampScale, 4.1) * weightsB.y +
            SampleBaseFast(RockBase, worldXZ, rockScale, 5.4) * weightsB.z;
    }
    else
    {
        albedo =
            SampleBase(GrassBase, worldXZ, grassScale, 0.2) * weightsA.x +
            SampleBase(LitterBase, worldXZ, litterScale, 1.1) * weightsA.y +
            SampleBase(PathBase, worldXZ, pathScale, 2.3) * weightsA.z +
            SampleBase(MudBase, worldXZ, mudScale, 3.2) * weightsB.x +
            SampleBase(SwampBase, worldXZ, swampScale, 4.1) * weightsB.y +
            SampleBase(RockBase, worldXZ, rockScale, 5.4) * weightsB.z;
    }

    float macro = MacroNoise(worldXZ * 0.62 + vec2(13.0, -7.0));
    albedo *= mix(0.88, 1.10, macro);
    albedo *= vec3(
        0.98 + 0.03 * macro,
        0.97 + 0.04 * macro,
        0.95 + 0.025 * macro);

    float terrainPbr = GraphicsFeatures2.x;
    vec3 baseNormal = normalize(fsin_WorldNormal);
    vec3 normal = baseNormal;
    float roughness = 0.82;
    float ao = 1.0;

    if (terrainDetail >= 1.0)
    {
        if (normalMapping > 0.5 && terrainPbr > 0.5)
        {
            vec3 tangentNormal =
                SampleNormal(GrassNormal, worldXZ, grassScale, 0.2) * weightsA.x +
                SampleNormal(LitterNormal, worldXZ, litterScale, 1.1) * weightsA.y +
                SampleNormal(PathNormal, worldXZ, pathScale, 2.3) * weightsA.z +
                SampleNormal(MudNormal, worldXZ, mudScale, 3.2) * weightsB.x +
                SampleNormal(SwampNormal, worldXZ, swampScale, 4.1) * weightsB.y +
                SampleNormal(RockNormal, worldXZ, rockScale, 5.4) * weightsB.z;
            normal = normalize(
                GroundTangentFrame(baseNormal) *
                normalize(tangentNormal));
        }

        if (terrainPbr > 0.5)
        {
            roughness =
                SampleScalar(GrassRoughness, worldXZ, grassScale, 0.2) * weightsA.x +
                SampleScalar(LitterRoughness, worldXZ, litterScale, 1.1) * weightsA.y +
                SampleScalar(PathRoughness, worldXZ, pathScale, 2.3) * weightsA.z +
                SampleScalar(MudRoughness, worldXZ, mudScale, 3.2) * weightsB.x +
                SampleScalar(SwampRoughness, worldXZ, swampScale, 4.1) * weightsB.y +
                SampleScalar(RockRoughness, worldXZ, rockScale, 5.4) * weightsB.z;
            roughness = clamp(roughness, 0.08, 1.0);

            ao =
                SampleScalar(GrassAo, worldXZ, grassScale, 0.2) * weightsA.x +
                SampleScalar(LitterAo, worldXZ, litterScale, 1.1) * weightsA.y +
                SampleScalar(PathAo, worldXZ, pathScale, 2.3) * weightsA.z +
                SampleScalar(MudAo, worldXZ, mudScale, 3.2) * weightsB.x +
                SampleScalar(SwampAo, worldXZ, swampScale, 4.1) * weightsB.y +
                SampleScalar(RockAo, worldXZ, rockScale, 5.4) * weightsB.z;
            ao = clamp(ao, 0.18, 1.0);
        }
    }
    vec3 sunDirection = normalize(Lighting.yzw);
    float ndotl = max(dot(normal, sunDirection), 0.0);
    float daylight = max(Lighting.x, 0.02);

    vec3 sunColor = SunColorTime.rgb;
    float cloudShadow = mix(
        1.0,
        CloudShadowFactor(
            fsin_WorldPosition,
            sunDirection,
            SkyWeather.w,
            SkyWeather.z,
            SkyWeather.x),
        GraphicsFeatures0.y);
    float geometryShadow = mix(
        1.0,
        SampleSunShadow(
            ShadowMap,
            ShadowSampler,
            LightViewProjection,
            fsin_WorldPosition,
            normal,
            sunDirection),
        GraphicsFeatures0.z);
    float directShadow = cloudShadow * geometryShadow;
    float hemisphere = mix(0.18, 0.64, clamp(normal.y * 0.5 + 0.5, 0.0, 1.0));

    float wetness = clamp(weightsB.x * 0.82 + weightsB.y * 0.66, 0.0, 1.0);
    float cavity = clamp(1.0 - normal.y, 0.0, 1.0);
    float geometricOcclusion = 1.0 - cavity * 0.14 - wetness * 0.06;
    float ambientOcclusion = clamp(ao * geometricOcclusion, 0.16, 1.0);

    float nightFactor = clamp(CelestialParameters.x, 0.0, 1.0);
    float terrainAmbient = mix(0.34, 0.055, nightFactor);
    vec3 diffuse = albedo *
        (hemisphere * terrainAmbient +
         ndotl * daylight * sunColor * directShadow) *
        ambientOcclusion;

    vec3 viewDirection = normalize(-fsin_WorldPosition);
    vec3 halfVector = normalize(sunDirection + viewDirection);
    float specPower = mix(5.0, 72.0, 1.0 - roughness);
    float specular = 0.0;
    float wetSpecular = 0.0;
    if (specularEnabled > 0.5 && terrainPbr > 0.5)
    {
        specular = pow(max(dot(normal, halfVector), 0.0), specPower);
        wetSpecular = wetness * (1.0 - roughness);
    }

    vec3 color = diffuse +
        sunColor * specular * wetSpecular * daylight * directShadow * 0.42;

    float density = max(FogColorDensity.w * GraphicsFeatures0.w, 0.00001);
    float fogFactor =
        1.0 - exp(-density * fsin_Distance * (1.0 + fsin_Distance * 0.0018));
    fogFactor = clamp(fogFactor, 0.0, 0.95);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    color = color / (color + vec3(0.75));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, 1.0);
}
