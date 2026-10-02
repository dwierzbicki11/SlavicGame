#version 450
const float PI = 3.14159265359;

layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
};

layout(set = 1, binding = 0) uniform texture2D GrassBase;
layout(set = 1, binding = 1) uniform texture2D GrassNormal;
layout(set = 1, binding = 2) uniform texture2D PathBase;
layout(set = 1, binding = 3) uniform texture2D PathNormal;
layout(set = 1, binding = 4) uniform texture2D LitterBase;
layout(set = 1, binding = 5) uniform texture2D LitterNormal;
layout(set = 1, binding = 6) uniform texture2D MudBase;
layout(set = 1, binding = 7) uniform texture2D MudNormal;
layout(set = 1, binding = 8) uniform texture2D SwampBase;
layout(set = 1, binding = 9) uniform texture2D SwampNormal;
layout(set = 1, binding = 10) uniform texture2D RockBase;
layout(set = 1, binding = 11) uniform texture2D RockNormal;
layout(set = 1, binding = 12) uniform sampler TerrainSampler;

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

vec3 SampleBase(texture2D tex, vec2 worldXZ, float metersPerTile, float seed)
{
    vec2 uvA = WarpedUv(worldXZ, metersPerTile, seed);
    vec2 uvB = Rotation(0.67 + seed * 0.11) *
               WarpedUv(worldXZ + vec2(17.0, -11.0), metersPerTile * 1.73, seed + 1.0);

    float blend = smoothstep(0.22, 0.78, MacroNoise(worldXZ + seed * 29.0));
    vec3 a = texture(sampler2D(tex, TerrainSampler), uvA).rgb;
    vec3 b = texture(sampler2D(tex, TerrainSampler), uvB).rgb;
    return mix(a, b, blend);
}

vec3 SampleNormal(texture2D tex, vec2 worldXZ, float metersPerTile, float seed)
{
    vec2 uv = WarpedUv(worldXZ, metersPerTile, seed);
    vec3 n = texture(sampler2D(tex, TerrainSampler), uv).xyz * 2.0 - 1.0;
    n.xy *= 0.82;
    return normalize(n);
}

mat3 GroundTangentFrame(vec3 n)
{
    vec3 tangent = normalize(vec3(1.0, -n.x / max(abs(n.y), 0.12), 0.0));
    vec3 bitangent = normalize(cross(n, tangent));
    return mat3(tangent, bitangent, n);
}

vec3 SunColor(vec3 sunDirection)
{
    float horizon = 1.0 - smoothstep(0.08, 0.48, max(sunDirection.y, 0.0));
    return mix(vec3(1.0, 0.95, 0.86), vec3(1.0, 0.47, 0.20), horizon * 0.82);
}

void main()
{
    vec3 weightsA = max(fsin_PrimaryWeights, vec3(0.0));
    vec3 weightsB = max(fsin_SecondaryWeights, vec3(0.0));

    // Break perfectly smooth biome borders with low-frequency world-space variation.
    float organic = MacroNoise(fsin_WorldPosition.xz);
    weightsA.x *= mix(0.93, 1.07, organic);
    weightsA.y *= mix(1.06, 0.94, organic);
    weightsB.x *= mix(0.92, 1.10, organic);
    weightsB.y *= mix(1.08, 0.94, organic);

    float weightSum =
        weightsA.x + weightsA.y + weightsA.z +
        weightsB.x + weightsB.y + weightsB.z;
    weightSum = max(weightSum, 0.0001);
    weightsA /= weightSum;
    weightsB /= weightSum;

    vec2 worldXZ = fsin_WorldPosition.xz;

    vec3 albedo =
        SampleBase(GrassBase, worldXZ, 2.1, 0.2) * weightsA.x +
        SampleBase(LitterBase, worldXZ, 2.0, 1.1) * weightsA.y +
        SampleBase(PathBase, worldXZ, 2.6, 2.3) * weightsA.z +
        SampleBase(MudBase, worldXZ, 2.4, 3.2) * weightsB.x +
        SampleBase(SwampBase, worldXZ, 2.8, 4.1) * weightsB.y +
        SampleBase(RockBase, worldXZ, 2.7, 5.4) * weightsB.z;

    // Large-scale tinting stops the ground from reading as a repeated wallpaper.
    float macro = MacroNoise(worldXZ * 0.62 + vec2(13.0, -7.0));
    albedo *= mix(0.88, 1.10, macro);
    albedo *= vec3(
        0.98 + 0.03 * macro,
        0.97 + 0.04 * macro,
        0.95 + 0.025 * macro);

    vec3 tangentNormal =
        SampleNormal(GrassNormal, worldXZ, 2.1, 0.2) * weightsA.x +
        SampleNormal(LitterNormal, worldXZ, 2.0, 1.1) * weightsA.y +
        SampleNormal(PathNormal, worldXZ, 2.6, 2.3) * weightsA.z +
        SampleNormal(MudNormal, worldXZ, 2.4, 3.2) * weightsB.x +
        SampleNormal(SwampNormal, worldXZ, 2.8, 4.1) * weightsB.y +
        SampleNormal(RockNormal, worldXZ, 2.7, 5.4) * weightsB.z;
    tangentNormal = normalize(tangentNormal);

    float roughness =
        0.78 * weightsA.x +
        0.82 * weightsA.y +
        0.88 * weightsA.z +
        0.46 * weightsB.x +
        0.56 * weightsB.y +
        0.74 * weightsB.z;
    roughness = clamp(roughness, 0.08, 1.0);

    vec3 baseNormal = normalize(fsin_WorldNormal);
    vec3 normal = normalize(GroundTangentFrame(baseNormal) * tangentNormal);
    vec3 sunDirection = normalize(Lighting.yzw);
    float ndotl = max(dot(normal, sunDirection), 0.0);
    float daylight = max(Lighting.x, 0.02);

    vec3 sunColor = SunColor(sunDirection);
    float hemisphere = mix(0.18, 0.62, clamp(normal.y * 0.5 + 0.5, 0.0, 1.0));

    // Slightly darken creases and wet lowland mixes to give the terrain depth.
    float wetness = clamp(weightsB.x * 0.82 + weightsB.y * 0.66, 0.0, 1.0);
    float cavity = clamp(1.0 - normal.y, 0.0, 1.0);
    float ambientOcclusion = 1.0 - cavity * 0.18 - wetness * 0.08;

    vec3 diffuse = albedo *
        (hemisphere * 0.34 + ndotl * daylight * sunColor) *
        ambientOcclusion;

    vec3 viewDirection = normalize(-fsin_WorldPosition);
    vec3 halfVector = normalize(sunDirection + viewDirection);
    float specPower = mix(5.0, 52.0, 1.0 - roughness);
    float specular = pow(max(dot(normal, halfVector), 0.0), specPower);

    // Only mud and swamp receive a visible wet highlight.
    vec3 color = diffuse + sunColor * specular * wetness * daylight * 0.28;

    float density = max(FogColorDensity.w, 0.00001);
    float fogFactor =
        1.0 - exp(-density * fsin_Distance * (1.0 + fsin_Distance * 0.0018));
    fogFactor = clamp(fogFactor, 0.0, 0.95);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    color = color / (color + vec3(0.75));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, 1.0);
}
