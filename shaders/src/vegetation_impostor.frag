#version 450

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

layout(location = 0) in vec2 fsin_TexCoord;
layout(location = 1) in vec4 fsin_ColorType;
layout(location = 2) in float fsin_Distance;
layout(location = 3) in vec3 fsin_WorldPosition;

layout(location = 0) out vec4 fsout_Color;

float Hash21(vec2 p)
{
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float Ellipse(vec2 uv, vec2 center, vec2 radius)
{
    vec2 p = (uv - center) / radius;
    return 1.0 - smoothstep(0.82, 1.03, dot(p, p));
}

float DeciduousWoodMask(vec2 uv)
{
    float x = uv.x - 0.5;
    float trunkWidth = mix(0.078, 0.027, clamp(uv.y / 0.58, 0.0, 1.0));
    float trunk =
        (1.0 - smoothstep(trunkWidth, trunkWidth + 0.024, abs(x))) *
        (1.0 - smoothstep(0.54, 0.62, uv.y));

    float branchHeight = clamp((uv.y - 0.35) / 0.30, 0.0, 1.0);
    float leftCenter = -0.03 - branchHeight * 0.22;
    float rightCenter = 0.04 + branchHeight * 0.24;
    float branchWidth = mix(0.035, 0.015, branchHeight);

    float leftBranch =
        (1.0 - smoothstep(
            branchWidth,
            branchWidth + 0.018,
            abs(x - leftCenter))) *
        smoothstep(0.34, 0.42, uv.y) *
        (1.0 - smoothstep(0.64, 0.72, uv.y));

    float rightBranch =
        (1.0 - smoothstep(
            branchWidth,
            branchWidth + 0.018,
            abs(x - rightCenter))) *
        smoothstep(0.36, 0.44, uv.y) *
        (1.0 - smoothstep(0.66, 0.74, uv.y));

    return max(trunk, max(leftBranch, rightBranch));
}

float DeciduousMask(vec2 uv)
{
    float wood = DeciduousWoodMask(uv);

    float crown = 0.0;
    crown = max(crown, Ellipse(uv, vec2(0.50, 0.69), vec2(0.43, 0.31)));
    crown = max(crown, Ellipse(uv, vec2(0.30, 0.63), vec2(0.27, 0.24)));
    crown = max(crown, Ellipse(uv, vec2(0.72, 0.65), vec2(0.29, 0.25)));
    crown = max(crown, Ellipse(uv, vec2(0.43, 0.84), vec2(0.28, 0.20)));
    crown = max(crown, Ellipse(uv, vec2(0.65, 0.86), vec2(0.24, 0.18)));
    crown = max(crown, Ellipse(uv, vec2(0.22, 0.78), vec2(0.20, 0.17)));

    // A few stable openings keep the crown from reading as one solid blob.
    float holeA = Ellipse(uv, vec2(0.42, 0.69), vec2(0.075, 0.060));
    float holeB = Ellipse(uv, vec2(0.64, 0.77), vec2(0.065, 0.052));
    float holeC = Ellipse(uv, vec2(0.28, 0.70), vec2(0.050, 0.042));
    crown = max(0.0, crown - max(holeA * 0.72, max(holeB * 0.66, holeC * 0.58)));

    float cells = Hash21(floor(uv * vec2(23.0, 29.0)));
    float breakup =
        (cells - 0.5) *
        0.16 *
        smoothstep(0.43, 0.72, crown);

    return max(wood, crown + breakup);
}

float PineMask(vec2 uv)
{
    float x = abs(uv.x - 0.5);
    float y = uv.y;

    float trunk =
        (1.0 - smoothstep(0.042, 0.070, x)) *
        (1.0 - smoothstep(0.70, 0.80, y));

    float lower = (1.0 - smoothstep(
        mix(0.45, 0.20, clamp((y - 0.18) / 0.34, 0.0, 1.0)),
        mix(0.48, 0.23, clamp((y - 0.18) / 0.34, 0.0, 1.0)),
        x)) *
        smoothstep(0.13, 0.21, y) *
        (1.0 - smoothstep(0.52, 0.58, y));

    float middle = (1.0 - smoothstep(
        mix(0.34, 0.13, clamp((y - 0.42) / 0.30, 0.0, 1.0)),
        mix(0.37, 0.16, clamp((y - 0.42) / 0.30, 0.0, 1.0)),
        x)) *
        smoothstep(0.38, 0.46, y) *
        (1.0 - smoothstep(0.73, 0.79, y));

    float upper = (1.0 - smoothstep(
        mix(0.24, 0.018, clamp((y - 0.64) / 0.36, 0.0, 1.0)),
        mix(0.27, 0.043, clamp((y - 0.64) / 0.36, 0.0, 1.0)),
        x)) *
        smoothstep(0.60, 0.68, y);

    float tierNoise = 0.93 + 0.07 * sin(y * 72.0);
    float crown = max(lower, max(middle, upper)) * tierNoise;

    float cells = Hash21(floor(uv * vec2(19.0, 31.0)));
    crown +=
        (cells - 0.5) *
        0.10 *
        smoothstep(0.28, 0.78, crown);

    return max(trunk, crown);
}

void main()
{
    float species = fsin_ColorType.w;
    float mask = species > 0.5 && species < 1.5
        ? PineMask(fsin_TexCoord)
        : DeciduousMask(fsin_TexCoord);

    // Break the old solid-card crown into small irregular gaps.  The holes are
    // stable in UV space, so they add depth without temporal shimmer.
    float canopyWeight = smoothstep(0.34, 0.70, fsin_TexCoord.y);
    float cellNoise = Hash21(
        floor(fsin_TexCoord * vec2(19.0, 23.0)) +
        vec2(species * 7.0, species * 13.0));
    float gapThreshold = species > 0.5 && species < 1.5 ? 0.965 : 0.925;
    float crownGap =
        smoothstep(gapThreshold, 0.995, cellNoise) *
        canopyWeight;
    mask *= 1.0 - crownGap * 0.92;

    if (mask < 0.38)
        discard;

    float woodZone = species > 0.5 && species < 1.5
        ? (1.0 - smoothstep(
            0.045,
            0.085,
            abs(fsin_TexCoord.x - 0.5))) *
          (1.0 - smoothstep(0.66, 0.78, fsin_TexCoord.y))
        : DeciduousWoodMask(fsin_TexCoord);

    vec3 trunkColor = species > 1.5 && species < 2.5
        ? vec3(0.60, 0.58, 0.50)
        : species > 0.5 && species < 1.5
            ? vec3(0.24, 0.15, 0.085)
            : vec3(0.20, 0.12, 0.065);

    float variation = Hash21(
        floor(fsin_TexCoord * vec2(29.0, 37.0)) +
        vec2(species * 13.7, species * 7.1));
    float macroVariation =
        0.93 +
        0.07 * sin(
            fsin_WorldPosition.x * 0.19 +
            fsin_WorldPosition.z * 0.13);

    vec3 leafColor =
        fsin_ColorType.rgb *
        mix(0.76, 1.16, variation) *
        macroVariation;

    vec3 baseColor = mix(
        leafColor,
        trunkColor,
        clamp(woodZone, 0.0, 1.0));

    float daylight = clamp(Lighting.x / 1.15, 0.0, 1.0);
    vec3 sunColor = SunColorTime.rgb;
    float verticalLight = mix(0.62, 1.03, fsin_TexCoord.y);
    float crownDepth =
        mix(0.89, 1.05, Hash21(floor(fsin_TexCoord * vec2(11.0, 13.0))));

    vec3 color =
        baseColor *
        crownDepth *
        (0.50 + daylight * verticalLight * sunColor * 0.58);

    float density = max(FogColorDensity.w * GraphicsFeatures0.w, 0.00001);
    float fogFactor = 1.0 - exp(-density * fsin_Distance);
    color = mix(
        color,
        FogColorDensity.rgb,
        clamp(fogFactor, 0.0, 0.94));

    fsout_Color = vec4(color, 1.0);
}
