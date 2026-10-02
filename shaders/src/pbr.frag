#version 450
const float PI = 3.14159265359;

layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
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

layout(location = 0) in vec3 fsin_ViewPosition;
layout(location = 1) in vec3 fsin_ViewNormal;
layout(location = 2) in vec2 fsin_TexCoord;
layout(location = 3) in float fsin_Distance;

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

    vec3 normal = normalize(fsin_ViewNormal);
    vec3 sampledNormal = texture(sampler2D(NormalTexture, MaterialSampler), fsin_TexCoord).xyz * 2.0 - 1.0;
    if (length(sampledNormal.xy) > 0.001)
        normal = normalize(CotangentFrame(normal, fsin_ViewPosition, fsin_TexCoord) * sampledNormal);

    vec3 mr = texture(sampler2D(MetallicRoughnessTexture, MaterialSampler), fsin_TexCoord).rgb;
    float metallic = clamp(MaterialFactors.x * mr.b, 0.0, 1.0);
    float roughness = clamp(MaterialFactors.y * mr.g, 0.06, 1.0);

    vec3 viewDirection = normalize(-fsin_ViewPosition);
    vec3 lightDirection = normalize(vec3(-0.35, 0.82, 0.28));
    vec3 halfway = normalize(viewDirection + lightDirection);

    float ndotl = max(dot(normal, lightDirection), 0.0);
    float ndotv = max(dot(normal, viewDirection), 0.0);

    vec3 f0 = mix(vec3(0.04), albedo, metallic);
    vec3 f = FresnelSchlick(max(dot(halfway, viewDirection), 0.0), f0);
    float d = DistributionGGX(normal, halfway, roughness);
    float g = GeometrySmith(normal, viewDirection, lightDirection, roughness);

    vec3 specular = (d * g * f) / max(4.0 * ndotv * ndotl, 0.001);
    vec3 kd = (vec3(1.0) - f) * (1.0 - metallic);
    vec3 diffuse = kd * albedo / PI;

    float lightStrength = max(Lighting.x, 0.18);
    vec3 ambient = albedo * (0.045 + 0.035 * (1.0 - metallic));
    vec3 color = ambient + (diffuse + specular) * ndotl * (1.7 * lightStrength);

    float fogFactor = 1.0 - exp(-FogColorDensity.w * fsin_Distance);
    fogFactor = clamp(fogFactor, 0.0, 0.94);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    color = color / (color + vec3(1.0));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, alpha);
}
