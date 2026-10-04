#ifndef SLAVICGAME_SHADOWS_GLSL
#define SLAVICGAME_SHADOWS_GLSL

float SampleSunShadow(
    texture2D shadowMap,
    sampler shadowSampler,
    mat4 lightViewProjection,
    vec3 worldPosition,
    vec3 worldNormal,
    vec3 sunDirection)
{
    vec4 lightClip =
        lightViewProjection * vec4(worldPosition, 1.0);

    if (lightClip.w <= 0.00001)
        return 1.0;

    vec3 projected = lightClip.xyz / lightClip.w;
    vec2 uv = projected.xy * 0.5 + 0.5;

    if (uv.x <= 0.001 || uv.x >= 0.999 ||
        uv.y <= 0.001 || uv.y >= 0.999 ||
        projected.z <= 0.0 || projected.z >= 1.0)
    {
        return 1.0;
    }

    float ndotl = max(
        dot(normalize(worldNormal), normalize(sunDirection)),
        0.0);
    float bias = max(0.00022, 0.00110 * (1.0 - ndotl));

    vec2 texel = 1.0 / vec2(
        textureSize(sampler2D(shadowMap, shadowSampler), 0));

    // Four symmetric taps keep a stable soft penumbra while cutting shadow-map
    // bandwidth by more than half versus the previous 3x3 (9 tap) kernel.
    // The offsets deliberately straddle the receiver instead of sampling the
    // centre so thin occluders still contribute to the filtered result.
    const vec2 offsets[4] = vec2[](
        vec2(-0.75, -0.75),
        vec2( 0.75, -0.75),
        vec2(-0.75,  0.75),
        vec2( 0.75,  0.75));

    float visible = 0.0;
    for (int i = 0; i < 4; i++)
    {
        float storedDepth = texture(
            sampler2D(shadowMap, shadowSampler),
            uv + offsets[i] * texel).r;
        visible += projected.z - bias <= storedDepth
            ? 1.0
            : 0.0;
    }

    // Never turn direct sunlight into pitch black. Indirect ambient light
    // remains the responsibility of the material shader.
    return mix(0.18, 1.0, visible * 0.25);
}

#endif
