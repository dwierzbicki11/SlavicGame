#version 450
layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
};

layout(location = 0) in vec3 fsin_WorldDirection;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    vec3 direction = normalize(fsin_WorldDirection);
    vec3 sunDirection = normalize(Lighting.yzw);
    float daylight = clamp(Lighting.x / 1.15, 0.0, 1.0);
    float sunHeight = max(sunDirection.y, 0.0);
    float twilight = 1.0 - smoothstep(0.08, 0.42, sunHeight);

    vec3 nightHorizon = vec3(0.012, 0.016, 0.028);
    vec3 nightZenith = vec3(0.003, 0.006, 0.018);
    vec3 dayHorizon = max(FogColorDensity.rgb * 1.10, vec3(0.12, 0.14, 0.16));
    vec3 dayZenith = vec3(0.055, 0.125, 0.235);

    vec3 horizon = mix(nightHorizon, dayHorizon, daylight);
    vec3 zenith = mix(nightZenith, dayZenith, daylight);

    vec3 sunsetHorizon = vec3(0.50, 0.16, 0.055);
    horizon = mix(horizon, sunsetHorizon, twilight * daylight * 0.55);

    float vertical = clamp(direction.y * 0.5 + 0.5, 0.0, 1.0);
    float gradient = pow(vertical, 0.72);
    vec3 color = mix(horizon, zenith, gradient);

    float sunDot = dot(direction, sunDirection);
    float sunDisc = smoothstep(0.99925, 0.99982, sunDot);
    float sunHalo = pow(max(sunDot, 0.0), mix(48.0, 112.0, sunHeight));
    vec3 sunColor = mix(vec3(1.00, 0.88, 0.62), vec3(1.00, 0.42, 0.12), twilight * 0.82);
    color += sunColor * (sunDisc * 2.35 + sunHalo * 0.22) * daylight;

    float horizonHaze = pow(1.0 - abs(direction.y), 5.0);
    color = mix(color, FogColorDensity.rgb, horizonHaze * 0.24);

    fsout_Color = vec4(color, 1.0);
}
