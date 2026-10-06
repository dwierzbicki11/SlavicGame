#version 450

layout(set = 0, binding = 0) uniform BloomParameters
{
    vec4 Params0; // texel.xy, threshold, mode
    vec4 Params1; // direction.xy, radius, reserved
};

layout(set = 0, binding = 1) uniform texture2D SourceColor;
layout(set = 0, binding = 2) uniform sampler BloomSampler;

layout(location = 0) in vec2 fsin_TexCoord;
layout(location = 0) out vec4 fsout_Color;

vec3 SampleSource(vec2 uv)
{
    return texture(
        sampler2D(SourceColor, BloomSampler),
        clamp(uv, vec2(0.0), vec2(1.0))).rgb;
}

vec3 BrightPart(vec3 color, float threshold)
{
    float peak = max(color.r, max(color.g, color.b));
    float knee = max(0.08, threshold * 0.25);
    float soft = clamp((peak - threshold + knee) / (2.0 * knee), 0.0, 1.0);
    soft = soft * soft * (3.0 - 2.0 * soft);
    float contribution = max(peak - threshold, 0.0) + soft * knee;
    return color * contribution / max(peak, 0.0001);
}

void main()
{
    vec2 texel = Params0.xy;
    float threshold = Params0.z;
    float mode = Params0.w;

    if (mode < 0.5)
    {
        vec2 halfTexel = texel * 0.5;
        vec3 c =
            SampleSource(fsin_TexCoord + vec2(-halfTexel.x, -halfTexel.y)) +
            SampleSource(fsin_TexCoord + vec2( halfTexel.x, -halfTexel.y)) +
            SampleSource(fsin_TexCoord + vec2(-halfTexel.x,  halfTexel.y)) +
            SampleSource(fsin_TexCoord + vec2( halfTexel.x,  halfTexel.y));

        fsout_Color = vec4(BrightPart(c * 0.25, threshold), 1.0);
        return;
    }

    vec2 direction = Params1.xy * texel * Params1.z;

    // Separable 9-tap Gaussian approximation.
    vec3 color = SampleSource(fsin_TexCoord) * 0.227027;
    color += SampleSource(fsin_TexCoord + direction * 1.384615) * 0.316216;
    color += SampleSource(fsin_TexCoord - direction * 1.384615) * 0.316216;
    color += SampleSource(fsin_TexCoord + direction * 3.230769) * 0.070270;
    color += SampleSource(fsin_TexCoord - direction * 3.230769) * 0.070270;

    fsout_Color = vec4(color, 1.0);
}
