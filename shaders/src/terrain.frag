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

vec3 FilmicToneMap(vec3 c)
{
    const float a = 2.51;
    const float b = 0.03;
    const float c1 = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return clamp((c * (a * c + b)) / (c * (c1 * c + d) + e), 0.0, 1.0);
}

float MacroNoise(vec2 p)
{
    float a = sin(p.x * 0.041 + p.y * 0.027);
    float b = cos(p.x * 0.019 - p.y * 0.053);
    float c = sin((p.x + p.y) * 0.013);
    return clamp(0.5 + a * 0.22 + b * 0.18 + c * 0.10, 0.0, 1.0);
}

// Per-material texture coordinates. Every map of one material uses the same
// warped/rotated UV pair and blend mask, so they are evaluated once per
// material instead of once per texture channel (previously ~7 trig ops for
// each of up to 5 channels). Sampling happens inside weight branches, so the
// gradients are supplied analytically from the uniform-flow world derivative.
struct LayerUv
{
    vec2 a;
    vec2 b;
    vec2 aDx;
    vec2 aDy;
    vec2 bDx;
    vec2 bDy;
    float blend;
};

LayerUv MakeLayerUv(vec2 worldXZ, vec2 dx, vec2 dy, float metersPerTile, float seed, bool dual)
{
    LayerUv layer;
    vec2 warp = vec2(
        sin(worldXZ.y * 0.021 + seed * 3.17),
        cos(worldXZ.x * 0.024 - seed * 2.31)) * 0.22;
    float invTile = 1.0 / metersPerTile;
    layer.a = (worldXZ + warp) * invTile;
    layer.aDx = dx * invTile;
    layer.aDy = dy * invTile;
    layer.b = layer.a;
    layer.bDx = layer.aDx;
    layer.bDy = layer.aDy;
    layer.blend = 0.0;
    if (dual)
    {
        float angle = 0.67 + seed * 0.11;
        float c = cos(angle);
        float s = sin(angle);
        mat2 rotation = mat2(c, -s, s, c);
        vec2 shifted = worldXZ + vec2(17.0, -11.0);
        float seedB = seed + 1.0;
        vec2 warpB = vec2(
            sin(shifted.y * 0.021 + seedB * 3.17),
            cos(shifted.x * 0.024 - seedB * 2.31)) * 0.22;
        float invTileB = invTile / 1.73;
        layer.b = rotation * ((shifted + warpB) * invTileB);
        layer.bDx = rotation * (dx * invTileB);
        layer.bDy = rotation * (dy * invTileB);
        layer.blend = smoothstep(0.22, 0.78, MacroNoise(worldXZ + seed * 29.0));
    }
    return layer;
}

vec4 SampleA(texture2D tex, LayerUv l)
{
    return textureGrad(sampler2D(tex, TerrainSampler), l.a, l.aDx, l.aDy);
}

vec4 SampleDual(texture2D tex, LayerUv l)
{
    return mix(
        textureGrad(sampler2D(tex, TerrainSampler), l.a, l.aDx, l.aDy),
        textureGrad(sampler2D(tex, TerrainSampler), l.b, l.bDx, l.bDy),
        l.blend);
}

vec3 SampleNormalA(texture2D tex, LayerUv l)
{
    vec3 a = textureGrad(sampler2D(tex, TerrainSampler), l.a, l.aDx, l.aDy).xyz * 2.0 - 1.0;
    a.xy *= 0.82;
    return normalize(a);
}

vec3 SampleNormalDual(texture2D tex, LayerUv l)
{
    vec3 a = textureGrad(sampler2D(tex, TerrainSampler), l.a, l.aDx, l.aDy).xyz * 2.0 - 1.0;
    vec3 b = textureGrad(sampler2D(tex, TerrainSampler), l.b, l.bDx, l.bDy).xyz * 2.0 - 1.0;
    a.xy *= 0.82;
    b.xy *= 0.82;
    return normalize(mix(a, b, l.blend));
}

mat3 GroundTangentFrame(vec3 n)
{
    vec3 tangent = normalize(vec3(1.0, -n.x / max(abs(n.y), 0.12), 0.0));
    vec3 bitangent = normalize(cross(n, tangent));
    return mat3(tangent, bitangent, n);
}

// Contributions below this normalized weight are barely perceptible (< 3.5 %)
// but cost a full set of texture fetches; they are dropped and renormalized.
const float LayerCutoff = 0.035;

struct LayerSurface
{
    vec3 albedo;
    vec3 tangentNormal;
    float roughness;
    float ao;
};

void main()
{
    vec3 weightsA = max(fsin_PrimaryWeights, vec3(0.0));
    vec3 weightsB = max(fsin_SecondaryWeights, vec3(0.0));
    vec2 worldXZ = fsin_WorldPosition.xz;
    vec2 worldDx = dFdx(worldXZ);
    vec2 worldDy = dFdy(worldXZ);

    float terrainDetail = GraphicsFeatures3.z;
    float normalMapping = GraphicsFeatures3.x;
    float specularEnabled = GraphicsFeatures3.y;
    float terrainPbr = GraphicsFeatures2.x;
    bool dual = (terrainDetail >= 2.0 && fsin_Distance < 90.0) || (terrainDetail >= 1.0 && fsin_Distance < 42.0);
    bool heightBlend = terrainDetail >= 2.0 && fsin_Distance < 55.0;
    bool wantNormal = normalMapping > 0.5 && terrainPbr > 0.5 && terrainDetail >= 1.0 && fsin_Distance < 36.0;
    bool wantPbr = terrainPbr > 0.5 && terrainDetail >= 1.0 && fsin_Distance < 48.0;

    float organic = MacroNoise(worldXZ);
    weightsA.x *= mix(0.93, 1.07, organic);
    weightsA.y *= mix(1.06, 0.94, organic);
    weightsB.x *= mix(0.92, 1.10, organic);
    weightsB.y *= mix(1.08, 0.94, organic);

    float weights[6] = float[](weightsA.x, weightsA.y, weightsA.z, weightsB.x, weightsB.y, weightsB.z);
    const float scales[6] = float[](2.1, 2.0, 2.6, 2.4, 2.8, 2.7);
    const float seeds[6] = float[](0.2, 1.1, 2.3, 3.2, 4.1, 5.4);

    float preSum = max(weights[0] + weights[1] + weights[2] + weights[3] + weights[4] + weights[5], 0.0001);
    for (int i = 0; i < 6; i++)
    {
        if (weights[i] < LayerCutoff * preSum)
            weights[i] = 0.0;
    }

    // UVs are rebuilt inside each branch rather than stored for all six
    // layers: keeping six LayerUv structs live would raise register pressure
    // enough to force narrower SIMD on older Intel/AMD iGPUs.
#define LAYER_UV(INDEX) MakeLayerUv(worldXZ, worldDx, worldDy, scales[INDEX], seeds[INDEX], dual)

    // Height-assisted blending (High/Ultra only).
    if (heightBlend)
    {
        if (weights[0] > 0.0) weights[0] *= mix(0.62, 1.38, SampleDual(GrassHeight, LAYER_UV(0)).r);
        if (weights[1] > 0.0) weights[1] *= mix(0.62, 1.38, SampleDual(LitterHeight, LAYER_UV(1)).r);
        if (weights[2] > 0.0) weights[2] *= mix(0.62, 1.38, SampleDual(PathHeight, LAYER_UV(2)).r);
        if (weights[3] > 0.0) weights[3] *= mix(0.62, 1.38, SampleDual(MudHeight, LAYER_UV(3)).r);
        if (weights[4] > 0.0) weights[4] *= mix(0.62, 1.38, SampleDual(SwampHeight, LAYER_UV(4)).r);
        if (weights[5] > 0.0) weights[5] *= mix(0.62, 1.38, SampleDual(RockHeight, LAYER_UV(5)).r);
    }

    float weightSum = max(weights[0] + weights[1] + weights[2] + weights[3] + weights[4] + weights[5], 0.0001);
    for (int i = 0; i < 6; i++)
        weights[i] /= weightSum;
    weightsA = vec3(weights[0], weights[1], weights[2]);
    weightsB = vec3(weights[3], weights[4], weights[5]);

    vec3 albedo = vec3(0.0);
    vec3 tangentNormal = vec3(0.0);
    float roughness = 0.0;
    float ao = 0.0;

#define TERRAIN_LAYER(INDEX, BASE, NORMAL, ROUGH, AO_MAP)                         \
    if (weights[INDEX] > 0.0)                                                     \
    {                                                                             \
        float w = weights[INDEX];                                                 \
        LayerUv l = LAYER_UV(INDEX);                                              \
        albedo += (dual ? SampleDual(BASE, l).rgb : SampleA(BASE, l).rgb) * w;    \
        if (wantNormal) tangentNormal += SampleNormalA(NORMAL, l) * w;            \
        if (wantPbr)                                                              \
        {                                                                         \
            roughness += SampleA(ROUGH, l).r * w;                                 \
            ao += SampleA(AO_MAP, l).r * w;                                       \
        }                                                                         \
    }

    TERRAIN_LAYER(0, GrassBase, GrassNormal, GrassRoughness, GrassAo)
    TERRAIN_LAYER(1, LitterBase, LitterNormal, LitterRoughness, LitterAo)
    TERRAIN_LAYER(2, PathBase, PathNormal, PathRoughness, PathAo)
    TERRAIN_LAYER(3, MudBase, MudNormal, MudRoughness, MudAo)
    TERRAIN_LAYER(4, SwampBase, SwampNormal, SwampRoughness, SwampAo)
    TERRAIN_LAYER(5, RockBase, RockNormal, RockRoughness, RockAo)
#undef TERRAIN_LAYER
#undef LAYER_UV

    float macro = MacroNoise(worldXZ * 0.62 + vec2(13.0, -7.0));
    albedo *= mix(0.88, 1.10, macro);
    albedo *= vec3(
        0.98 + 0.03 * macro,
        0.97 + 0.04 * macro,
        0.95 + 0.025 * macro);
    float rainWetness = clamp(SkyWeather.y, 0.0, 1.0);
    albedo *= mix(1.0, 0.78, rainWetness);

    vec3 baseNormal = normalize(fsin_WorldNormal);
    vec3 normal = baseNormal;
    if (wantNormal)
    {
        normal = normalize(
            GroundTangentFrame(baseNormal) *
            normalize(tangentNormal));
    }

    if (wantPbr)
    {
        roughness = clamp(roughness, 0.08, 1.0);
        roughness = mix(
            roughness,
            max(0.12, roughness * 0.48),
            rainWetness);
        ao = clamp(ao, 0.18, 1.0);
    }
    else
    {
        roughness = 0.82;
        ao = 1.0;
    }
    vec3 sunDirection = normalize(Lighting.yzw);
    float ndotl = max(dot(normal, sunDirection), 0.0);
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
    if (GraphicsFeatures0.z > 0.5 && ndotl > 0.001)
    {
        geometryShadow = SampleSunShadow(
            ShadowMap,
            ShadowSampler,
            LightViewProjection,
            fsin_WorldPosition,
            baseNormal,
            sunDirection);
    }
    float directShadow = cloudShadow * geometryShadow;
    float hemisphere = mix(0.18, 0.64, clamp(normal.y * 0.5 + 0.5, 0.0, 1.0));

    float materialWetness = clamp(
        weightsB.x * 0.82 + weightsB.y * 0.66,
        0.0,
        1.0);
    float wetness = max(materialWetness, rainWetness * 0.82);
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

    float fogEnabled = GraphicsFeatures0.w;
    float density = max(FogColorDensity.w * fogEnabled, 0.00001);
    float fogFactor =
        1.0 - exp(-density * fsin_Distance * (1.0 + fsin_Distance * 0.0018));

    // Snująca się słowiańska mgła przyziemna w dolinach i nad Czarnymi Mokradłami
    float swampWeight = weightsB.y;
    float twilightFactor = clamp(CelestialParameters.z, 0.0, 1.0);
    float lowGround = clamp((3.2 - fsin_WorldPosition.y) * 0.26 + swampWeight * 0.38, 0.0, 1.0);
    float groundMist = lowGround *
                       smoothstep(10.0, 56.0, fsin_Distance) *
                       fogEnabled *
                       mix(0.24, 0.48, max(twilightFactor, nightFactor));
    fogFactor = clamp(max(fogFactor, groundMist), 0.0, 0.95);

    vec3 viewRay = normalize(fsin_WorldPosition);
    float sunScatter = pow(max(dot(viewRay, sunDirection), 0.0), 5.0) * (1.0 - nightFactor);
    vec3 slavicFogColor = mix(FogColorDensity.rgb, sunColor * vec3(1.02, 0.78, 0.52), sunScatter * 0.34);
    color = mix(color, slavicFogColor, fogFactor);

    color = FilmicToneMap(color);
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, 1.0);
}
