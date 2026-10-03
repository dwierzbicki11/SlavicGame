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

    sampler2D sampledShadow = sampler2D(shadowMap, shadowSampler);
    vec2 texel = 1.0 / vec2(textureSize(sampledShadow, 0));

    float visible = 0.0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float storedDepth = texture(
                sampledShadow,
                uv + vec2(x, y) * texel).r;
            visible += projected.z - bias <= storedDepth
                ? 1.0
                : 0.0;
        }
    }

    // Never turn direct sunlight into pitch black. Indirect ambient light
    // remains the responsibility of the material shader.
    return mix(0.18, 1.0, visible / 9.0);
}

#endif
