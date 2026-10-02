#version 450
layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
};

layout(location = 0) in vec3 fsin_Color;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) in vec3 fsin_WorldPosition;
layout(location = 3) in float fsin_Distance;

layout(location = 0) out vec4 fsout_Color;

float Hash21(vec2 p)
{
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float ValueNoise(vec2 p)
{
    vec2 cell = floor(p);
    vec2 fraction = fract(p);
    fraction = fraction * fraction * (3.0 - 2.0 * fraction);

    float a = Hash21(cell);
    float b = Hash21(cell + vec2(1.0, 0.0));
    float c = Hash21(cell + vec2(0.0, 1.0));
    float d = Hash21(cell + vec2(1.0, 1.0));

    return mix(mix(a, b, fraction.x), mix(c, d, fraction.x), fraction.y);
}

void main()
{
    vec3 normal = normalize(fsin_Normal);
    vec3 sunDirection = normalize(vec3(-0.35, 0.82, 0.28));

    float direct = max(dot(normal, sunDirection), 0.0);
    float hemisphere = mix(0.30, 0.72, clamp(normal.y * 0.5 + 0.5, 0.0, 1.0));
    float slope = 1.0 - clamp(normal.y, 0.0, 1.0);

    float macro = ValueNoise(fsin_WorldPosition.xz * 0.045);
    float micro = ValueNoise(fsin_WorldPosition.xz * 0.28);
    float surfaceVariation = mix(0.88, 1.10, macro) * mix(0.95, 1.05, micro);

    vec3 soilTint = vec3(0.22, 0.17, 0.105);
    vec3 baseColor = mix(fsin_Color, soilTint, slope * slope * 0.32);
    baseColor *= surfaceVariation;

    float daylight = max(Lighting.x, 0.20);
    vec3 ambient = baseColor * hemisphere * (0.40 + daylight * 0.24);
    vec3 sun = baseColor * direct * daylight * 0.92;
    vec3 color = ambient + sun;

    float density = max(FogColorDensity.w, 0.00001);
    float fogFactor = 1.0 - exp(-density * fsin_Distance * (1.0 + fsin_Distance * 0.0018));
    fogFactor = clamp(fogFactor, 0.0, 0.95);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    color = color / (color + vec3(0.75));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, 1.0);
}
