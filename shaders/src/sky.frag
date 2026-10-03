#version 450
const float PI = 3.14159265359;
const float TWO_PI = 6.28318530718;

layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
    vec4 SunColorTime;
    vec4 SkyWeather;
    vec4 MoonParameters;
    vec4 CelestialParameters;
};

layout(location = 0) in vec3 fsin_WorldDirection;
layout(location = 0) out vec4 fsout_Color;

float Hash12(vec2 p)
{
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

vec2 Hash22(vec2 p)
{
    float n = Hash12(p);
    return fract(vec2(n, n * 1.2154 + 0.371));
}

float Noise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);

    float a = Hash12(i);
    float b = Hash12(i + vec2(1.0, 0.0));
    float c = Hash12(i + vec2(0.0, 1.0));
    float d = Hash12(i + vec2(1.0, 1.0));

    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float Fbm(vec2 p)
{
    float value = 0.0;
    float amplitude = 0.52;

    for (int octave = 0; octave < 5; octave++)
    {
        value += Noise(p) * amplitude;
        p = mat2(1.63, -1.11, 1.11, 1.63) * p + 13.7;
        amplitude *= 0.50;
    }

    return value;
}

vec3 AtmosphericBase(vec3 direction, vec3 sunDirection, float daylight, float nightFactor)
{
    float up = clamp(direction.y, -0.15, 1.0);
    float horizon = pow(1.0 - clamp(up, 0.0, 1.0), 2.35);

    vec3 nightZenith = vec3(0.0025, 0.0055, 0.0160);
    vec3 nightHorizon = vec3(0.010, 0.015, 0.030);

    vec3 dayZenith = vec3(0.035, 0.115, 0.260);
    vec3 dayHorizon = vec3(0.36, 0.48, 0.62);

    vec3 nightSky = mix(nightZenith, nightHorizon, horizon);
    vec3 daySky = mix(dayZenith, dayHorizon, horizon);

    float sunElevation = sunDirection.y;
    float twilightBand =
        exp(-abs(sunElevation) * 7.5) *
        smoothstep(-0.16, 0.12, sunElevation);
    float towardSun = pow(max(dot(direction, sunDirection), 0.0), 5.0);
    float horizonFacingSun = horizon * mix(0.32, 1.0, towardSun);

    vec3 twilightColor = SunColorTime.rgb * vec3(1.0, 0.72, 0.48);
    daySky = mix(
        daySky,
        twilightColor,
        clamp(twilightBand * horizonFacingSun * 0.86, 0.0, 0.92));

    float dayMix = clamp(daylight * 1.22, 0.0, 1.0);
    return mix(nightSky, daySky, dayMix * (1.0 - nightFactor * 0.18));
}

float StarField(vec3 direction, float timeSeconds)
{
    vec2 spherical = vec2(
        atan(direction.z, direction.x) / TWO_PI + 0.5,
        asin(clamp(direction.y, -1.0, 1.0)) / PI + 0.5);

    vec2 grid = spherical * vec2(520.0, 260.0);
    vec2 cell = floor(grid);
    vec2 local = fract(grid) - 0.5;
    vec2 point = (Hash22(cell) - 0.5) * 0.70;

    float seed = Hash12(cell + 17.3);
    float starMask = step(0.986, seed);
    float radius = mix(0.055, 0.018, fract(seed * 31.71));
    float star = 1.0 - smoothstep(radius * 0.45, radius, length(local - point));
    float brightness = mix(0.30, 1.45, fract(seed * 71.17));
    float twinkle = 0.90 + 0.10 * sin(timeSeconds * 0.72 + seed * 93.0);

    return star * starMask * brightness * twinkle;
}

vec3 RenderMoon(vec3 direction, vec3 moonDirection, float phase)
{
    float moonDot = dot(direction, moonDirection);
    float disc = smoothstep(0.99974, 0.99993, moonDot);
    if (disc <= 0.0)
        return vec3(0.0);

    vec3 reference = abs(moonDirection.y) > 0.92
        ? vec3(1.0, 0.0, 0.0)
        : vec3(0.0, 1.0, 0.0);
    vec3 tangent = normalize(cross(reference, moonDirection));
    vec3 bitangent = normalize(cross(moonDirection, tangent));
    vec2 local = vec2(dot(direction, tangent), dot(direction, bitangent)) * 78.0;

    float crater =
        Fbm(local * 3.2 + vec2(11.7, -4.1)) * 0.60 +
        Fbm(local * 7.5 - vec2(2.8, 13.2)) * 0.40;
    crater = mix(0.64, 1.02, crater);

    float phaseOffset = mix(-0.85, 0.85, clamp(phase, 0.0, 1.0));
    float litSide = smoothstep(-0.18, 0.12, local.x + phaseOffset);
    float radial = clamp(1.0 - dot(local, local), 0.0, 1.0);
    float sphereShade = 0.48 + 0.52 * sqrt(radial);

    vec3 moonBase = vec3(0.72, 0.77, 0.82) * crater * sphereShade;
    return moonBase * mix(0.13, 1.0, litSide) * disc;
}

float CloudDensity(vec3 direction, float timeSeconds, float wind, float cloudiness)
{
    float skyMask = smoothstep(-0.04, 0.22, direction.y);
    if (skyMask <= 0.0 || cloudiness <= 0.01)
        return 0.0;

    float perspective = max(direction.y + 0.32, 0.24);
    vec2 uv = direction.xz / perspective;
    vec2 windOffset = vec2(0.018, 0.010) *
        timeSeconds * mix(0.25, 1.55, wind);

    float broad = Fbm(uv * 0.72 + windOffset);
    float detail = Fbm(uv * 1.85 - windOffset * 1.7 + 21.3);
    float density = broad * 0.78 + detail * 0.22;

    float threshold = mix(0.80, 0.39, cloudiness);
    float softness = mix(0.08, 0.18, cloudiness);
    return smoothstep(threshold, threshold + softness, density) * skyMask;
}

void main()
{
    vec3 direction = normalize(fsin_WorldDirection);
    vec3 sunDirection = normalize(Lighting.yzw);
    vec3 moonDirection = normalize(MoonParameters.xyz);

    float sunIntensity = max(Lighting.x, 0.0);
    float daylight = clamp(sunIntensity / 1.15, 0.0, 1.0);
    float cloudiness = clamp(SkyWeather.x, 0.0, 1.0);
    float rain = clamp(SkyWeather.y, 0.0, 1.0);
    float wind = clamp(SkyWeather.z, 0.0, 1.0);
    float timeSeconds = SkyWeather.w;
    float nightFactor = clamp(CelestialParameters.x, 0.0, 1.0);
    float lunarPhase = clamp(CelestialParameters.y, 0.0, 1.0);

    vec3 color = AtmosphericBase(direction, sunDirection, daylight, nightFactor);

    // Physically motivated forward scattering around the sun.
    float sunDot = max(dot(direction, sunDirection), 0.0);
    float mieGlow = pow(sunDot, 22.0);
    float tightGlow = pow(sunDot, 280.0);
    float sunDisc = smoothstep(0.99984, 0.999965, dot(direction, sunDirection));
    float sunAboveHorizon = smoothstep(-0.035, 0.025, sunDirection.y);
    vec3 sunColor = SunColorTime.rgb;

    color += sunColor *
        (mieGlow * 0.11 + tightGlow * 0.38 + sunDisc * 3.8) *
        max(sunIntensity, 0.10) *
        sunAboveHorizon;

    // Dense procedural star catalogue. Daylight, moon glow and clouds wash it out.
    float stars = StarField(direction, timeSeconds);
    float starVisibility =
        pow(nightFactor, 1.65) *
        smoothstep(-0.03, 0.18, direction.y) *
        (1.0 - cloudiness * 0.92);
    vec3 starColor = mix(
        vec3(0.72, 0.80, 1.0),
        vec3(1.0, 0.89, 0.68),
        Hash12(floor(direction.xz * 317.0)));
    color += starColor * stars * starVisibility;

    // Moon follows the opposite celestial arc to the sun.
    vec3 moon = RenderMoon(direction, moonDirection, lunarPhase);
    float moonVisibility =
        MoonParameters.w *
        smoothstep(-0.035, 0.025, moonDirection.y) *
        (1.0 - cloudiness * 0.82);
    float moonHalo = pow(max(dot(direction, moonDirection), 0.0), 96.0);
    color += moon * moonVisibility * 2.5;
    color += vec3(0.34, 0.43, 0.58) * moonHalo * moonVisibility * 0.20;

    float clouds = CloudDensity(direction, timeSeconds, wind, cloudiness);
    if (clouds > 0.0)
    {
        float sunCloudLight =
            0.24 +
            max(dot(direction, sunDirection), 0.0) * daylight * 0.76;
        vec3 dayCloud = mix(
            vec3(0.34, 0.37, 0.40),
            vec3(0.90, 0.90, 0.86),
            sunCloudLight);
        vec3 sunsetCloud = SunColorTime.rgb * vec3(0.92, 0.68, 0.52);
        dayCloud = mix(
            dayCloud,
            sunsetCloud,
            CelestialParameters.z * 0.58);

        vec3 nightCloud = vec3(0.035, 0.045, 0.065) +
            vec3(0.08, 0.10, 0.15) * MoonParameters.w;
        vec3 cloudColor = mix(nightCloud, dayCloud, clamp(daylight * 1.3, 0.0, 1.0));
        cloudColor *= mix(1.0, 0.52, rain);

        float cloudOpacity = clouds * mix(0.46, 0.90, cloudiness);
        color = mix(color, cloudColor, cloudOpacity);
    }

    float horizonHaze = pow(1.0 - abs(clamp(direction.y, -1.0, 1.0)), 5.0);
    float hazeStrength = mix(0.16, 0.36, cloudiness) +
        clamp(FogColorDensity.w / 0.032, 0.0, 1.0) * 0.28;
    color = mix(color, FogColorDensity.rgb, horizonHaze * hazeStrength);

    // Keep sky HDR enough for bright celestial discs, then compress gently.
    color = color / (color + vec3(0.72));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, 1.0);
}
