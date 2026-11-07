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
    vec4 GraphicsFeatures3;
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
layout(location = 4) in vec3 fsin_ViewVector;

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

vec3 EvaluateSlavicFireLight(vec3 worldPos, vec3 normal, float timeSeconds)
{
    const vec2 anchors[6] = vec2[](
        vec2(0.0, -88.0),    // Ognisko w Żarnowcu
        vec2(37.0, 21.0),    // Ognisko myśliwskie w puszczy
        vec2(-6.5, 6.5),     // Kapliczka na polanie startowej
        vec2(-5.5, -66.0),   // Kapliczka na rozdrożu Żarnowca
        vec2(-85.0, 64.5),   // Słup Światowida w Kamiennym Kręgu
        vec2(68.0, 22.0)     // Totem Welesa przed Czarnymi Mokradłami
    );
    const float radii[6] = float[](11.5, 9.5, 5.2, 5.5, 6.8, 5.5);

    vec3 total = vec3(0.0);
    for (int i = 0; i < 6; i++)
    {
        vec3 lightPos = vec3(anchors[i].x, worldPos.y + 0.35, anchors[i].y);
        vec3 toLight = lightPos - worldPos;
        float dist = length(toLight);
        float radius = radii[i];
        if (dist < radius)
        {
            float atten = max(1.0 - dist / radius, 0.0);
            atten = atten * atten;
            float flicker = 0.88 + 0.12 * sin(timeSeconds * 8.4 + float(i) * 2.1);
            float ndotl = max(dot(normal, toLight / max(dist, 0.001)) * 0.65 + 0.35, 0.0);
            total += vec3(1.55, 0.68, 0.18) * (atten * flicker * ndotl * 0.48);
        }
    }
    return total;
}

void main()
{
    vec3 geomNormal = normalize(fsin_Normal);
    vec3 viewDir = normalize(fsin_ViewVector);
    vec3 sunDirection = normalize(Lighting.yzw);
    float daylight = max(Lighting.x, 0.02);
    float nightFactor = clamp(CelestialParameters.x, 0.0, 1.0);
    float twilightFactor = clamp(CelestialParameters.z, 0.0, 1.0);
    float rainWetness = clamp(SkyWeather.y, 0.0, 1.0);

    vec3 baseColor = min(fsin_Color, vec3(1.0));
    vec3 emissive = max(fsin_Color - vec3(1.0), vec3(0.0));

    // Heuristic material response from character palette (skin, metal, leather, hair, linen/wool)
    float luma = dot(baseColor, vec3(0.299, 0.587, 0.114));
    float chroma = max(baseColor.r, max(baseColor.g, baseColor.b)) - min(baseColor.r, min(baseColor.g, baseColor.b));
    float isMetal = smoothstep(0.11, 0.03, chroma) * smoothstep(0.32, 0.55, luma);
    float isSkin = smoothstep(0.36, 0.58, luma) *
                   smoothstep(0.04, 0.16, baseColor.r - baseColor.g) *
                   smoothstep(0.03, 0.14, baseColor.g - baseColor.b);
    float isLeather = (1.0 - isSkin) *
                      smoothstep(0.42, 0.16, luma) *
                      smoothstep(0.03, 0.14, baseColor.r - baseColor.b);
    float isCloth = clamp(1.0 - max(isMetal, max(isSkin, isLeather)), 0.0, 1.0);

    // Subtle procedural cloth/leather/hair micro-detail (fades smoothly with distance to prevent aliasing)
    float detailFade = smoothstep(34.0, 4.5, fsin_Distance);
    vec3 p = fsin_WorldPosition * 36.0;
    float weaveA = sin(p.x + p.y * 0.65) * cos(p.z - p.y * 0.55);
    float weaveB = sin(p.x * 2.1 - p.z * 1.9 + p.y * 1.4);
    float skinPore = sin(p.x * 3.4 + p.y * 3.1) * cos(p.z * 3.4 - p.y * 2.8) * 0.22;
    float microGrain = mix(weaveA * 0.65 + weaveB * 0.35, skinPore, isSkin * 0.80) * detailFade;

    baseColor *= 1.0 + microGrain * 0.052;
    baseColor *= mix(1.0, 0.85, rainWetness * (1.0 - isMetal * 0.6));

    vec3 microPerturb = vec3(
        cos(p.x + p.z * 0.5),
        sin(p.y * 1.2) * 0.4,
        sin(p.z - p.x * 0.5)) * (microGrain * mix(0.085, 0.035, isSkin));
    vec3 normal = normalize(geomNormal + microPerturb);

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
            geomNormal,
            sunDirection);
    }
    float directShadow = cloudShadow * geometryShadow;

    // 1. Half-Lambert + Subsurface Wrap Diffuse (softens harsh polygon terminators on faces & tunics)
    float rawNdotL = dot(normal, sunDirection);
    float ndotl = max(rawNdotL, 0.0);
    float halfLambert = clamp(rawNdotL * 0.5 + 0.5, 0.0, 1.0);
    float wrapDiffuse = mix(ndotl, halfLambert * halfLambert, 0.36);

    float terminatorBand = smoothstep(0.0, 0.48, halfLambert) * (1.0 - ndotl);
    vec3 sssTint = mix(vec3(0.85, 0.44, 0.24), vec3(1.00, 0.42, 0.22), isSkin);
    vec3 subsurface = baseColor * sssTint * terminatorBand * mix(0.12, 0.30, isSkin);

    // 2. Hemispherical 3-Point Ambient (Sky dome + Warm ground bounce + Back-fill)
    float skyHemi = clamp(normal.y * 0.5 + 0.5, 0.0, 1.0);
    float groundHemi = clamp(-normal.y * 0.5 + 0.5, 0.0, 1.0);
    float backFill = max(dot(normal, -sunDirection), 0.0);

    vec3 skyAmbientColor = mix(
        mix(vec3(0.36, 0.42, 0.50), vec3(0.45, 0.33, 0.28), twilightFactor),
        vec3(0.055, 0.075, 0.12),
        nightFactor);
    vec3 groundBounceColor = mix(
        vec3(0.24, 0.21, 0.15),
        vec3(0.025, 0.030, 0.035),
        nightFactor);

    vec3 hemiAmbient = baseColor * (
        skyAmbientColor * (0.38 + 0.62 * skyHemi) +
        groundBounceColor * groundHemi * 0.52 +
        sunColor * backFill * mix(0.11, 0.02, nightFactor));

    // Subtle vertical grounding gradient across lower boots/hem
    float cavityShade = mix(0.86, 1.06, skyHemi);
    hemiAmbient *= cavityShade;

    // 3. Specular Sheen (dual-lobe skin sheen, forged steel, leather belts/boots, and woven cloth fuzz)
    vec3 halfway = normalize(viewDir + sunDirection);
    float ndoth = max(dot(normal, halfway), 0.0);
    float ndotv = max(dot(normal, viewDir), 0.0);
    float specPower = mix(mix(20.0, 36.0, isLeather), 68.0, isMetal);
    specPower = mix(specPower, 28.0, isSkin);
    specPower = mix(specPower, 48.0, rainWetness * 0.65);
    float specStrength = mix(mix(0.06, 0.20, isLeather), 0.54, isMetal);
    specStrength = mix(specStrength, 0.15, isSkin);
    specStrength = mix(specStrength, max(specStrength, 0.22), rainWetness);
    vec3 specTint = mix(vec3(1.0), baseColor * 1.25, isMetal * 0.65);
    float skinSecondaryLobe = isSkin * pow(ndoth, 7.0) * 0.055;
    float clothVelvetSheen = isCloth * pow(1.0 - ndotv, 2.6) * 0.085;
    vec3 specular = sunColor * (specTint * (pow(ndoth, specPower) * specStrength + skinSecondaryLobe) + baseColor * clothVelvetSheen) * ndotl * directShadow * daylight;

    // 4. Fresnel Silhouette Rim Light (separates character silhouettes cleanly from dark foliage)
    float fresnel = pow(1.0 - ndotv, 3.2) * smoothstep(-0.25, 0.55, normal.y);
    vec3 rimLightColor = mix(sunColor * 0.65 + skyAmbientColor * 0.55, vec3(0.18, 0.24, 0.36), nightFactor);
    vec3 rimLight = rimLightColor * fresnel * mix(0.24, 0.10, nightFactor) * (0.35 + 0.65 * directShadow);

    // 5. Moonlit night fill + warm Slavic campfire/kapliczka glow
    vec3 moonDir = normalize(MoonParameters.xyz);
    float moonNdotL = max(dot(normal, moonDir) * 0.5 + 0.5, 0.0);
    vec3 moonLight = baseColor * vec3(0.32, 0.42, 0.62) * moonNdotL * MoonParameters.w * nightFactor * 0.28;
    vec3 fireLight = baseColor * EvaluateSlavicFireLight(fsin_WorldPosition, normal, SkyWeather.w);

    vec3 directLight = (baseColor * wrapDiffuse + subsurface) * sunColor * daylight * directShadow * 0.90;
    vec3 color = hemiAmbient + directLight + specular + rimLight + moonLight + fireLight + emissive * 0.95;

    float fogEnabled = GraphicsFeatures0.w;
    float density = max(FogColorDensity.w * fogEnabled, 0.00001);
    float groundMist = clamp((2.6 - fsin_WorldPosition.y) * 0.24, 0.0, 1.0) *
                       smoothstep(8.0, 48.0, fsin_Distance) *
                       fogEnabled *
                       mix(0.18, 0.38, max(twilightFactor, nightFactor));
    float fogFactor = 1.0 - exp(-density * fsin_Distance);
    fogFactor = clamp(max(fogFactor, groundMist), 0.0, 0.94);

    float sunScatter = pow(max(dot(-viewDir, sunDirection), 0.0), 5.0) * (1.0 - nightFactor);
    vec3 slavicFogColor = mix(FogColorDensity.rgb, sunColor * vec3(1.02, 0.78, 0.52), sunScatter * 0.32);
    color = mix(color, slavicFogColor, fogFactor);

    color = FilmicToneMap(color);
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, 1.0);
}
