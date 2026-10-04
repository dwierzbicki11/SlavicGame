#version 450

layout(set = 0, binding = 0) uniform TemporalMatrices
{
    mat4 InverseCurrentViewProjection;
    mat4 PreviousViewProjection;
    vec4 TemporalParameters;
};

layout(set = 0, binding = 1) uniform texture2D SceneDepth;
layout(set = 0, binding = 2) uniform sampler PointSampler;

layout(location = 0) in vec2 fsin_TexCoord;
layout(location = 0) out vec2 fsout_Motion;

void main()
{
    float depth = texture(
        sampler2D(SceneDepth, PointSampler),
        fsin_TexCoord).r;

    if (TemporalParameters.w > 0.5 || depth >= 0.999999)
    {
        fsout_Motion = vec2(0.0);
        return;
    }

    float clipYSign = TemporalParameters.z;
    vec2 currentNdc = fsin_TexCoord * 2.0 - 1.0;
    currentNdc.y *= clipYSign;

    vec4 currentClip = vec4(currentNdc, depth, 1.0);
    vec4 world = InverseCurrentViewProjection * currentClip;
    world /= max(abs(world.w), 0.000001);

    vec4 previousClip = PreviousViewProjection * world;
    if (abs(previousClip.w) <= 0.000001)
    {
        fsout_Motion = vec2(0.0);
        return;
    }

    vec2 previousNdc = previousClip.xy / previousClip.w;
    previousNdc.y *= clipYSign;

    vec2 previousUv = previousNdc * 0.5 + 0.5;

    // FSR consumes screen-space motion. Store normalized UV displacement so
    // the backend can apply its documented render-size scale exactly once.
    fsout_Motion = fsin_TexCoord - previousUv;
}
