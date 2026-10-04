#version 450

layout(set = 0, binding = 0) uniform DynamicMotionMatrices
{
    mat4 CurrentViewProjection;
    mat4 PreviousViewProjection;
    vec4 MotionParameters;
};

layout(location = 0) in vec3 CurrentPosition;
layout(location = 1) in vec3 PreviousPosition;

layout(location = 0) noperspective out vec2 fsin_Motion;

void main()
{
    vec4 currentClip =
        CurrentViewProjection * vec4(CurrentPosition, 1.0);
    vec4 previousClip =
        PreviousViewProjection * vec4(PreviousPosition, 1.0);

    gl_Position = currentClip;

    if (MotionParameters.w > 0.5 ||
        abs(currentClip.w) <= 0.000001 ||
        abs(previousClip.w) <= 0.000001)
    {
        fsin_Motion = vec2(0.0);
        return;
    }

    float clipYSign = MotionParameters.z;
    vec2 currentNdc = currentClip.xy / currentClip.w;
    vec2 previousNdc = previousClip.xy / previousClip.w;
    currentNdc.y *= clipYSign;
    previousNdc.y *= clipYSign;

    vec2 currentUv = currentNdc * 0.5 + 0.5;
    vec2 previousUv = previousNdc * 0.5 + 0.5;
    fsin_Motion = currentUv - previousUv;
}
