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

layout(set = 1, binding = 0) uniform MaterialBuffer
{
    vec4 BaseColorFactor;
    vec4 MaterialFactors;
};

layout(set = 1, binding = 1) uniform texture2D BaseColorTexture;
layout(set = 1, binding = 2) uniform texture2D NormalTexture;
layout(set = 1, binding = 3) uniform texture2D MetallicRoughnessTexture;
layout(set = 1, binding = 4) uniform sampler MaterialSampler;

layout(set = 2, binding = 0) uniform ShadowDataBuffer
{
    mat4 LightViewProjection;
};
layout(set = 2, binding = 1) uniform texture2D ShadowMap;
layout(set = 2, binding = 2) uniform sampler ShadowSampler;

layout(location = 0) in vec3 fsin_WorldPosition;
layout(location = 1) in vec3 fsin_WorldNormal;
layout(location = 2) in vec2 fsin_TexCoord;
layout(location = 3) in float fsin_Distance;
layout(location = 4) in vec3 fsin_CameraPosition;

layout(location = 0) out vec4 fsout_Color;

mat3 CotangentFrame(vec3 normal, vec3 position, vec2 uv)
{
    vec3 dp1 = dFdx(position);
    vec3 dp2 = dFdy(position);
    vec2 duv1 = dFdx(uv);
    vec2 duv2 = dFdy(uv);

    vec3 dp2perp = cross(dp2, normal);
    vec3 dp1perp = cross(normal, dp1);
    vec3 tangent = dp2perp * duv1.x + dp1perp * duv2.x;
    vec3 bitangent = dp2perp * duv1.y + dp1perp * duv2.y;

    float invmax = inversesqrt(max(dot(tangent, tangent), dot(bitangent, bitangent)) + 1e-8);
    return mat3(tangent * invmax, bitangent * invmax, normal);
}

float DistributionGGX(vec3 n, vec3 h, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float ndoth = max(dot(n, h), 0.0);
    float ndoth2 = ndoth * ndoth;
    float denom = ndoth2 * (a2 - 1.0) + 1.0;
    return a2 / max(PI * denom * denom, 0.0001);
}

float GeometrySchlickGGX(float ndotv, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;
    return ndotv / max(ndotv * (1.0 - k) + k, 0.0001);
}

float GeometrySmith(vec3 n, vec3 v, vec3 l, float roughness)
{
    return GeometrySchlickGGX(max(dot(n, v), 0.0), roughness)
         * GeometrySchlickGGX(max(dot(n, l), 0.0), roughness);
}

vec3 FresnelSchlick(float cosTheta, vec3 f0)
{
    return f0 + (1.0 - f0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}


void main()
{
    vec4 baseSample = texture(sampler2D(BaseColorTexture, MaterialSampler), fsin_TexCoord);
    vec3 albedo = max(baseSample.rgb * BaseColorFactor.rgb, vec3(0.0));
    float alpha = baseSample.a * BaseColorFactor.a;

    float modelPbr = GraphicsFeatures2.y;
    float normalMapping = GraphicsFeatures3.x;
    float specularEnabled = GraphicsFeatures3.y;

    vec3 baseNormal = normalize(fsin_WorldNormal);
    vec3 normal = baseNormal;
    if (modelPbr > 0.5 && normalMapping > 0.5)
    {
        vec3 sampledNormal = texture(
            sampler2D(NormalTexture, MaterialSampler),
            fsin_TexCoord).xyz * 2.0 - 1.0;
        if (length(sampledNormal.xy) > 0.001)
        {
            vec3 detailedNormal = normalize(
                CotangentFrame(baseNormal, fsin_WorldPosition, fsin_TexCoord) *
                sampledNormal);
            normal = detailedNormal;
        }
    }

    float metallic = 0.0;
    float roughness = 0.82;
    if (modelPbr > 0.5 && specularEnabled > 0.5)
    {
        vec3 mr = texture(
            sampler2D(MetallicRoughnessTexture, MaterialSampler),
            fsin_TexCoord).rgb;
        metallic = clamp(MaterialFactors.x * mr.b, 0.0, 1.0);
        roughness = clamp(MaterialFactors.y * mr.g, 0.06, 1.0);
    }

    vec3 viewDirection = normalize(fsin_CameraPosition - fsin_WorldPosition);
    vec3 lightDirection = normalize(Lighting.yzw);
    vec3 halfway = normalize(viewDirection + lightDirection);

    float ndotl = max(dot(normal, lightDirection), 0.0);
    float ndotv = max(dot(normal, viewDirection), 0.0);

    vec3 specular = vec3(0.0);
    vec3 diffuse = albedo / PI;
    if (modelPbr > 0.5 && specularEnabled > 0.5)
    {
        vec3 f0 = mix(vec3(0.04), albedo, metallic);
        vec3 f = FresnelSchlick(max(dot(halfway, viewDirection), 0.0), f0);
        float d = DistributionGGX(normal, halfway, roughness);
        float g = GeometrySmith(normal, viewDirection, lightDirection, roughness);
        specular = (d * g * f) / max(4.0 * ndotv * ndotl, 0.001);
        vec3 kd = (vec3(1.0) - f) * (1.0 - metallic);
        diffuse = kd * albedo / PI;
    }

    float lightStrength = max(Lighting.x, 0.02);
    vec3 sunColor = SunColorTime.rgb;
    float cloudShadow = mix(
        1.0,
        CloudShadowFactor(
            fsin_WorldPosition,
            lightDirection,
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
            lightDirection),
        GraphicsFeatures0.z);
    float directShadow = cloudShadow * geometryShadow;
    float nightFactor = clamp(CelestialParameters.x, 0.0, 1.0);
    float ambientStrength = mix(
        0.028 + 0.050 * max(lightDirection.y, 0.0),
        0.006,
        nightFactor);
    vec3 ambient = albedo * ambientStrength * (1.0 - metallic * 0.35);
    vec3 simpleDiffuse = albedo / PI;
    float fullPbr = modelPbr * specularEnabled;
    vec3 directBrdf = mix(
        simpleDiffuse,
        diffuse + specular,
        fullPbr);
    vec3 color = ambient +
        directBrdf * sunColor * ndotl *
        (1.75 * lightStrength) * directShadow;

    float fogFactor = 1.0 - exp(
        -(FogColorDensity.w * GraphicsFeatures0.w) * fsin_Distance);
    fogFactor = clamp(fogFactor, 0.0, 0.94);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    color = color / (color + vec3(1.0));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, alpha);
}
