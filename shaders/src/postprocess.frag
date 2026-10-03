#version 450

layout(set = 0, binding = 0) uniform PostProcessParameters
{
    vec4 Params0;
    vec4 Params1;
};

layout(set = 0, binding = 1) uniform texture2D SceneColor;
layout(set = 0, binding = 2) uniform sampler SceneSampler;

layout(location = 0) in vec2 fsin_TexCoord;
layout(location = 0) out vec4 fsout_Color;

float Luma(vec3 c)
{
    return dot(c, vec3(0.299, 0.587, 0.114));
}

vec3 SampleScene(vec2 uv)
{
    return texture(
        sampler2D(SceneColor, SceneSampler),
        clamp(uv, vec2(0.0), vec2(1.0))).rgb;
}

vec3 ApplyFxaa(vec2 uv, vec2 texel)
{
    vec3 rgbM = SampleScene(uv);
    float lumaM = Luma(rgbM);

    float lumaN = Luma(SampleScene(uv + vec2(0.0, -texel.y)));
    float lumaS = Luma(SampleScene(uv + vec2(0.0, texel.y)));
    float lumaW = Luma(SampleScene(uv + vec2(-texel.x, 0.0)));
    float lumaE = Luma(SampleScene(uv + vec2(texel.x, 0.0)));

    float lumaMin = min(
        lumaM,
        min(min(lumaN, lumaS), min(lumaW, lumaE)));
    float lumaMax = max(
        lumaM,
        max(max(lumaN, lumaS), max(lumaW, lumaE)));

    float range = lumaMax - lumaMin;
    if (range < max(0.0312, lumaMax * 0.125))
        return rgbM;

    vec2 dir = vec2(
        -(lumaN - lumaS),
        lumaW - lumaE);

    float reduce = max(
        (lumaN + lumaS + lumaW + lumaE) * 0.0078125,
        0.0009765625);
    float inverseMin = 1.0 /
        (min(abs(dir.x), abs(dir.y)) + reduce);

    dir = clamp(
        dir * inverseMin,
        vec2(-8.0),
        vec2(8.0)) * texel;

    vec3 rgbA = 0.5 * (
        SampleScene(uv + dir * (1.0 / 3.0 - 0.5)) +
        SampleScene(uv + dir * (2.0 / 3.0 - 0.5)));

    vec3 rgbB = rgbA * 0.5 + 0.25 * (
        SampleScene(uv + dir * -0.5) +
        SampleScene(uv + dir * 0.5));

    float lumaB = Luma(rgbB);
    return (lumaB < lumaMin || lumaB > lumaMax)
        ? rgbA
        : rgbB;
}

vec3 BrightPart(vec3 color, float threshold)
{
    float peak = max(color.r, max(color.g, color.b));
    float amount = max(peak - threshold, 0.0) /
        max(peak, 0.0001);
    return color * amount;
}

vec3 ComputeBloom(vec2 uv, vec2 texel, int taps, float threshold)
{
    if (taps <= 0)
        return vec3(0.0);

    const vec2 directions[12] = vec2[](
        vec2(1.0, 0.0),
        vec2(-1.0, 0.0),
        vec2(0.0, 1.0),
        vec2(0.0, -1.0),
        vec2(0.707, 0.707),
        vec2(-0.707, 0.707),
        vec2(0.707, -0.707),
        vec2(-0.707, -0.707),
        vec2(2.0, 0.5),
        vec2(-2.0, -0.5),
        vec2(0.5, 2.0),
        vec2(-0.5, -2.0)
    );

    float radius = taps <= 4
        ? 2.0
        : taps <= 8
            ? 3.5
            : 5.0;

    vec3 sum = vec3(0.0);
    float weight = 0.0;

    for (int i = 0; i < 12; i++)
    {
        if (i >= taps)
            break;

        vec2 offset = directions[i] * texel * radius;
        float w = 1.0 / (1.0 + dot(directions[i], directions[i]));
        sum += BrightPart(SampleScene(uv + offset), threshold) * w;
        weight += w;
    }

    return weight > 0.0 ? sum / weight : vec3(0.0);
}

void main()
{
    vec2 texel = Params0.xy;
    bool fxaa = Params0.z > 0.5;
    int bloomTaps = int(Params0.w + 0.5);

    float bloomStrength = Params1.x;
    float bloomThreshold = Params1.y;
    float brightness = Params1.z;
    float gamma = max(Params1.w, 0.01);

    vec3 color = fxaa
        ? ApplyFxaa(fsin_TexCoord, texel)
        : SampleScene(fsin_TexCoord);

    if (bloomTaps > 0 && bloomStrength > 0.001)
    {
        color += ComputeBloom(
            fsin_TexCoord,
            texel,
            bloomTaps,
            bloomThreshold) * bloomStrength;
    }

    color *= brightness;

    // Existing scene shaders output display-gamma color. Gamma=2.2 therefore
    // leaves it unchanged, while lower/higher values provide user adjustment.
    color = pow(
        max(color, vec3(0.0)),
        vec3(2.2 / gamma));

    fsout_Color = vec4(color, 1.0);
}
