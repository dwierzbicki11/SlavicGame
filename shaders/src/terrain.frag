#version 450
const float PI = 3.14159265359;

layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
};

layout(set = 1, binding = 0) uniform texture2D GrassBase;
layout(set = 1, binding = 1) uniform texture2D GrassNormal;
layout(set = 1, binding = 2) uniform texture2D GrassRoughness;

layout(set = 1, binding = 3) uniform texture2D PathBase;
layout(set = 1, binding = 4) uniform texture2D PathNormal;
layout(set = 1, binding = 5) uniform texture2D PathRoughness;

layout(set = 1, binding = 6) uniform texture2D LitterBase;
layout(set = 1, binding = 7) uniform texture2D LitterNormal;
layout(set = 1, binding = 8) uniform texture2D LitterRoughness;

layout(set = 1, binding = 9) uniform texture2D MudBase;
layout(set = 1, binding = 10) uniform texture2D MudNormal;
layout(set = 1, binding = 11) uniform texture2D MudRoughness;

layout(set = 1, binding = 12) uniform texture2D SwampBase;
layout(set = 1, binding = 13) uniform texture2D SwampNormal;
layout(set = 1, binding = 14) uniform texture2D SwampRoughness;

layout(set = 1, binding = 15) uniform texture2D RockBase;
layout(set = 1, binding = 16) uniform texture2D RockNormal;
layout(set = 1, binding = 17) uniform texture2D RockRoughness;

layout(set = 1, binding = 18) uniform sampler TerrainSampler;

layout(location = 0) in vec3 fsin_WorldPosition;
layout(location = 1) in vec3 fsin_WorldNormal;
layout(location = 2) in vec3 fsin_PrimaryWeights;
layout(location = 3) in vec3 fsin_SecondaryWeights;
layout(location = 4) in float fsin_Distance;

layout(location = 0) out vec4 fsout_Color;

vec3 DecodeNormal(texture2D tex, vec2 uv)
{
    return texture(sampler2D(tex, TerrainSampler), uv).xyz * 2.0 - 1.0;
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
    float weightSum =
        weightsA.x + weightsA.y + weightsA.z +
        weightsB.x + weightsB.y + weightsB.z;
    weightSum = max(weightSum, 0.0001);
    weightsA /= weightSum;
    weightsB /= weightSum;

    vec2 grassUv = fsin_WorldPosition.xz / 2.0;
    vec2 pathUv = fsin_WorldPosition.xz / 2.5;
    vec2 litterUv = fsin_WorldPosition.xz / 2.0;
    vec2 mudUv = fsin_WorldPosition.xz / 2.5;
    vec2 swampUv = fsin_WorldPosition.xz / 2.5;
    vec2 rockUv = fsin_WorldPosition.xz / 2.5;

    vec3 albedo =
        texture(sampler2D(GrassBase, TerrainSampler), grassUv).rgb * weightsA.x +
        texture(sampler2D(LitterBase, TerrainSampler), litterUv).rgb * weightsA.y +
        texture(sampler2D(PathBase, TerrainSampler), pathUv).rgb * weightsA.z +
        texture(sampler2D(MudBase, TerrainSampler), mudUv).rgb * weightsB.x +
        texture(sampler2D(SwampBase, TerrainSampler), swampUv).rgb * weightsB.y +
        texture(sampler2D(RockBase, TerrainSampler), rockUv).rgb * weightsB.z;

    vec3 tangentNormal =
        DecodeNormal(GrassNormal, grassUv) * weightsA.x +
        DecodeNormal(LitterNormal, litterUv) * weightsA.y +
        DecodeNormal(PathNormal, pathUv) * weightsA.z +
        DecodeNormal(MudNormal, mudUv) * weightsB.x +
        DecodeNormal(SwampNormal, swampUv) * weightsB.y +
        DecodeNormal(RockNormal, rockUv) * weightsB.z;
    tangentNormal = normalize(tangentNormal);

    float roughness =
        texture(sampler2D(GrassRoughness, TerrainSampler), grassUv).r * weightsA.x +
        texture(sampler2D(LitterRoughness, TerrainSampler), litterUv).r * weightsA.y +
        texture(sampler2D(PathRoughness, TerrainSampler), pathUv).r * weightsA.z +
        texture(sampler2D(MudRoughness, TerrainSampler), mudUv).r * weightsB.x +
        texture(sampler2D(SwampRoughness, TerrainSampler), swampUv).r * weightsB.y +
        texture(sampler2D(RockRoughness, TerrainSampler), rockUv).r * weightsB.z;
    roughness = clamp(roughness, 0.08, 1.0);

    vec3 baseNormal = normalize(fsin_WorldNormal);
    vec3 normal = normalize(GroundTangentFrame(baseNormal) * tangentNormal);
    vec3 sunDirection = normalize(Lighting.yzw);
    float ndotl = max(dot(normal, sunDirection), 0.0);
    float daylight = max(Lighting.x, 0.02);

    vec3 sunColor = SunColor(sunDirection);
    float hemisphere = mix(0.18, 0.62, clamp(normal.y * 0.5 + 0.5, 0.0, 1.0));
    vec3 diffuse = albedo * (hemisphere * 0.34 + ndotl * daylight * sunColor);

    // A restrained wet-specular response makes mud/swamp surfaces feel damp
    // without turning the whole terrain into plastic.
    vec3 viewDirection = normalize(-fsin_WorldPosition);
    vec3 halfVector = normalize(sunDirection + viewDirection);
    float specPower = mix(5.0, 42.0, 1.0 - roughness);
    float specular = pow(max(dot(normal, halfVector), 0.0), specPower);
    float wetness = weightsB.x * 0.75 + weightsB.y * 0.55;
    vec3 color = diffuse + sunColor * specular * wetness * daylight * 0.22;

    float density = max(FogColorDensity.w, 0.00001);
    float fogFactor =
        1.0 - exp(-density * fsin_Distance * (1.0 + fsin_Distance * 0.0018));
    fogFactor = clamp(fogFactor, 0.0, 0.95);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    color = color / (color + vec3(0.75));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, 1.0);
}
