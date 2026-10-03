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

float DeciduousMask(vec2 uv)
{
    vec2 p = vec2(uv.x - 0.5, uv.y);

    float trunkWidth = mix(0.075, 0.025, clamp(uv.y / 0.55, 0.0, 1.0));
    float trunk = (1.0 - smoothstep(trunkWidth, trunkWidth + 0.025, abs(p.x))) *
        (1.0 - smoothstep(0.50, 0.58, uv.y));

    vec2 c0 = vec2(p.x / 0.44, (uv.y - 0.62) / 0.34);
    vec2 c1 = vec2((p.x + 0.19) / 0.31, (uv.y - 0.58) / 0.27);
    vec2 c2 = vec2((p.x - 0.20) / 0.32, (uv.y - 0.60) / 0.28);
    vec2 c3 = vec2(p.x / 0.30, (uv.y - 0.82) / 0.24);

    float crown = max(
        max(1.0 - smoothstep(0.82, 1.02, dot(c0, c0)),
            1.0 - smoothstep(0.82, 1.02, dot(c1, c1))),
        max(1.0 - smoothstep(0.82, 1.02, dot(c2, c2)),
            1.0 - smoothstep(0.82, 1.02, dot(c3, c3))));

    return max(trunk, crown);
}

float PineMask(vec2 uv)
{
    float x = abs(uv.x - 0.5);
    float y = uv.y;

    float trunk = (1.0 - smoothstep(0.045, 0.075, x)) *
        (1.0 - smoothstep(0.55, 0.65, y));

    float crownY = clamp((y - 0.16) / 0.84, 0.0, 1.0);
    float width = mix(0.48, 0.025, crownY);
    width *= 0.86 + 0.14 * sin(y * 58.0);
    float crown = (1.0 - smoothstep(width, width + 0.025, x)) *
        smoothstep(0.10, 0.20, y);

    return max(trunk, crown);
}

void main()
{
    float species = fsin_ColorType.w;
    float mask = species > 0.5 && species < 1.5
        ? PineMask(fsin_TexCoord)
        : DeciduousMask(fsin_TexCoord);

    if (mask < 0.38)
        discard;

    float trunkZone =
        (1.0 - smoothstep(0.06, 0.11, abs(fsin_TexCoord.x - 0.5))) *
        (1.0 - smoothstep(0.42, 0.58, fsin_TexCoord.y));

    vec3 trunkColor = species > 1.5 && species < 2.5
        ? vec3(0.58, 0.56, 0.48)
        : vec3(0.20, 0.12, 0.065);

    float variation = Hash21(
        floor(fsin_WorldPosition.xz * 0.23) +
        fsin_TexCoord * 17.0);
    vec3 leafColor = fsin_ColorType.rgb *
        mix(0.78, 1.18, variation);

    vec3 baseColor = mix(
        leafColor,
        trunkColor,
        clamp(trunkZone, 0.0, 1.0));

    float daylight = clamp(Lighting.x / 1.15, 0.0, 1.0);
    vec3 sunColor = SunColorTime.rgb;
    float verticalLight = mix(0.68, 1.0, fsin_TexCoord.y);
    vec3 color = baseColor *
        (0.52 + daylight * verticalLight * sunColor * 0.56);

    float density = max(FogColorDensity.w * GraphicsFeatures0.w, 0.00001);
    float fogFactor = 1.0 - exp(-density * fsin_Distance);
    color = mix(
        color,
        FogColorDensity.rgb,
        clamp(fogFactor, 0.0, 0.94));

    fsout_Color = vec4(color, 1.0);
}
