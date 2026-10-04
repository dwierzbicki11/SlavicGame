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

layout(set = 1, binding = 0) uniform MaterialBuffer
{
    vec4 BaseColorFactor;
    vec4 MaterialFactors;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;
layout(location = 3) in float WindWeight;

layout(location = 0) out vec3 fsin_WorldPosition;
layout(location = 1) out vec3 fsin_WorldNormal;
layout(location = 2) out vec2 fsin_TexCoord;
layout(location = 3) out float fsin_Distance;
layout(location = 4) out vec3 fsin_CameraPosition;

vec3 ApplyFoliageWind(vec3 position, vec2 uv)
{
    float response =
        max(MaterialFactors.z, 0.0) *
        clamp(WindWeight, 0.0, 1.0);
    float wind = clamp(SkyWeather.z, 0.0, 1.0);
    if (response <= 0.001 || wind <= 0.001)
        return position;

    float timeSeconds = SkyWeather.w;
    float spatialPhase =
        position.x * 0.071 +
        position.z * 0.053 +
        position.y * 0.017;

    float gust =
        0.66 +
        0.22 * sin(timeSeconds * 0.43 + spatialPhase * 0.71) +
        0.12 * sin(timeSeconds * 0.19 - spatialPhase * 1.27);
    gust = clamp(gust, 0.28, 1.0);

    float sway =
        sin(timeSeconds * (0.72 + wind * 0.82) + spatialPhase) * 0.5 + 0.5;
    float flutter =
        sin(
            timeSeconds * (4.8 + wind * 3.6) +
            spatialPhase * 3.2 +
            uv.x * 7.3 +
            uv.y * 5.1);

    vec2 windDirection = normalize(vec2(
        0.84 + sin(timeSeconds * 0.17) * 0.10,
        0.54 + cos(timeSeconds * 0.13) * 0.10));

    float bend =
        response *
        wind *
        mix(0.055, 0.31, gust) *
        mix(0.72, 1.0, sway);

    position.xz += windDirection * bend;
    position.y += flutter * response * wind * 0.032;

    return position;
}

void main()
{
    vec3 worldPosition = ApplyFoliageWind(Position, TexCoord);
    vec4 viewPosition = View * vec4(worldPosition, 1.0);
    gl_Position = Projection * viewPosition;

    fsin_WorldPosition = worldPosition;
    fsin_WorldNormal = normalize(Normal);
    fsin_TexCoord = TexCoord;
    fsin_Distance = length(viewPosition.xyz);
    fsin_CameraPosition = inverse(View)[3].xyz;
}
