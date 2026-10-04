#version 450
layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

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

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoord;
layout(location = 2) in vec4 ColorType;
layout(location = 3) in float WindPhase;

layout(location = 0) out vec2 fsin_TexCoord;
layout(location = 1) out vec4 fsin_ColorType;
layout(location = 2) out float fsin_Distance;
layout(location = 3) out vec3 fsin_WorldPosition;

float SpeciesResponse(float species)
{
    if (species > 0.5 && species < 1.5)
        return 0.58;
    if (species > 1.5 && species < 2.5)
        return 1.15;
    if (species > 2.5)
        return 0.92;
    return 0.82;
}

void main()
{
    vec3 worldPosition = Position;
    float wind = clamp(SkyWeather.z, 0.0, 1.0);

    if (wind > 0.001)
    {
        float timeSeconds = SkyWeather.w;
        float heightWeight = smoothstep(0.08, 1.0, TexCoord.y);
        float response = SpeciesResponse(ColorType.w);

        float gust =
            0.68 +
            0.20 * sin(timeSeconds * 0.41 + WindPhase) +
            0.12 * sin(timeSeconds * 0.17 - WindPhase * 1.37);
        gust = clamp(gust, 0.30, 1.0);

        float sway =
            sin(timeSeconds * (0.70 + wind * 0.80) + WindPhase * 1.13);
        float flutter =
            sin(
                timeSeconds * (4.4 + wind * 3.8) +
                WindPhase * 3.7 +
                TexCoord.x * 7.1 +
                TexCoord.y * 4.9);

        vec2 windDirection = normalize(vec2(
            0.84 + sin(timeSeconds * 0.17) * 0.10,
            0.54 + cos(timeSeconds * 0.13) * 0.10));

        float bend =
            heightWeight *
            response *
            wind *
            mix(0.07, 0.34, gust) *
            (0.84 + sway * 0.16);

        worldPosition.xz += windDirection * bend;
        worldPosition.y +=
            flutter *
            heightWeight *
            response *
            wind *
            0.035;
    }

    vec4 viewPosition = View * vec4(worldPosition, 1.0);
    gl_Position = Projection * viewPosition;

    fsin_TexCoord = TexCoord;
    fsin_ColorType = ColorType;
    fsin_Distance = length(viewPosition.xyz);
    fsin_WorldPosition = worldPosition;
}
