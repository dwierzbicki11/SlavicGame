#ifndef SLAVICGAME_CLOUDS_GLSL
#define SLAVICGAME_CLOUDS_GLSL

float CloudHash12(vec2 p)
{
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float CloudNoise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);

    float a = CloudHash12(i);
    float b = CloudHash12(i + vec2(1.0, 0.0));
    float c = CloudHash12(i + vec2(0.0, 1.0));
    float d = CloudHash12(i + vec2(1.0, 1.0));

    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float CloudFbm(vec2 p)
{
    float value = 0.0;
    float amplitude = 0.535;

    for (int octave = 0; octave < 4; octave++)
    {
        value += CloudNoise(p) * amplitude;
        p = mat2(1.63, -1.11, 1.11, 1.63) * p + 13.7;
        amplitude *= 0.50;
    }

    return value;
}

float CloudFbmFast(vec2 p)
{
    float value = 0.0;
    float amplitude = 0.57;

    for (int octave = 0; octave < 3; octave++)
    {
        value += CloudNoise(p) * amplitude;
        p = mat2(1.63, -1.11, 1.11, 1.63) * p + 13.7;
        amplitude *= 0.50;
    }

    return value;
}

vec2 CloudWindOffset(float timeSeconds, float wind)
{
    return vec2(0.0105, 0.0062) *
        timeSeconds *
        mix(0.30, 1.85, clamp(wind, 0.0, 1.0));
}

float CloudCoverageField(
    vec2 worldXZ,
    float timeSeconds,
    float wind,
    float cloudiness)
{
    if (cloudiness <= 0.005)
        return 0.0;

    float threshold = mix(0.78, 0.39, clamp(cloudiness, 0.0, 1.0));
    vec2 motion = CloudWindOffset(timeSeconds, wind);
    vec2 baseUv = worldXZ * 0.00062 + motion;
    float broad = CloudFbm(baseUv) * 0.68;
    if (broad + 0.34 <= threshold)
        return 0.0;

    float detail = CloudFbmFast(baseUv * 2.35 - motion * 1.75 + 19.7);
    float wisps = CloudNoise(baseUv * 5.10 + motion * 0.55 - 7.4);

    float field = broad + detail * 0.23 + wisps * 0.09;
    float softness = mix(0.10, 0.19, cloudiness);
    return smoothstep(threshold, threshold + softness, field);
}

float CloudVolumeDensity(
    vec3 worldPoint,
    float timeSeconds,
    float wind,
    float cloudiness)
{
    const float cloudBase = 1450.0;
    const float cloudTop = 3050.0;

    float h = (worldPoint.y - cloudBase) / (cloudTop - cloudBase);
    if (h <= 0.0 || h >= 1.0 || cloudiness <= 0.005)
        return 0.0;

    float threshold = mix(0.80, 0.40, clamp(cloudiness, 0.0, 1.0));
    vec2 motion = CloudWindOffset(timeSeconds, wind);
    vec2 p = worldPoint.xz * 0.00068 + motion;
    p += vec2(h * 0.31, -h * 0.23);

    float shape = CloudFbm(p) * 0.70;
    if (shape + 0.32 <= threshold)
        return 0.0;

    float heightEnvelope =
        smoothstep(0.0, 0.16, h) *
        (1.0 - smoothstep(0.70, 1.0, h));

    shape += CloudFbmFast(p * 2.10 - motion * 1.40 + 23.1) * 0.22;
    shape += CloudNoise(p * 5.40 + vec2(h * 1.7, -h * 1.2)) * 0.08;

    float density = smoothstep(threshold, threshold + 0.16, shape);
    return density * heightEnvelope;
}

float CloudVolumeDensityFast(
    vec3 worldPoint,
    float timeSeconds,
    float wind,
    float cloudiness)
{
    const float cloudBase = 1450.0;
    const float cloudTop = 3050.0;

    float h = (worldPoint.y - cloudBase) / (cloudTop - cloudBase);
    if (h <= 0.0 || h >= 1.0 || cloudiness <= 0.005)
        return 0.0;

    float threshold = mix(0.80, 0.40, clamp(cloudiness, 0.0, 1.0));
    vec2 motion = CloudWindOffset(timeSeconds, wind);
    vec2 p = worldPoint.xz * 0.00068 + motion + vec2(h * 0.31, -h * 0.23);

    float shape = CloudFbmFast(p) * 0.76;
    if (shape + 0.26 <= threshold)
        return 0.0;

    shape += CloudNoise(p * 2.10 - motion * 1.40 + 23.1) * 0.24;
    float heightEnvelope =
        smoothstep(0.0, 0.16, h) *
        (1.0 - smoothstep(0.70, 1.0, h));
    return smoothstep(threshold, threshold + 0.16, shape) * heightEnvelope;
}

vec2 CloudRaymarch(
    vec3 rayDirection,
    vec3 sunDirection,
    float timeSeconds,
    float wind,
    float cloudiness,
    int requestedSteps)
{
    if (rayDirection.y <= 0.015 || cloudiness <= 0.005)
        return vec2(0.0, 1.0);

    const float cloudBase = 1450.0;
    const float cloudTop = 3050.0;
    const int maxSteps = 20;
    int steps = clamp(requestedSteps, 4, maxSteps);

    float tNear = cloudBase / rayDirection.y;
    float tFar = cloudTop / rayDirection.y;
    float stepLength = (tFar - tNear) / float(steps);

    float transmittance = 1.0;
    float accumulatedLight = 0.0;
    float accumulatedWeight = 0.0;

    float jitter = CloudHash12(floor(rayDirection.xz * 8192.0));
    float t = tNear + stepLength * jitter;

    for (int i = 0; i < maxSteps; i++)
    {
        if (i >= steps)
            break;

        vec3 point = rayDirection * t;
        float density = CloudVolumeDensity(
            point,
            timeSeconds,
            wind,
            cloudiness);

        if (density > 0.001)
        {
            float sunProbe = CloudVolumeDensityFast(
                point + sunDirection * 260.0,
                timeSeconds,
                wind,
                cloudiness);
            float selfShadow = exp(-sunProbe * 2.2);
            float powder = 1.0 - exp(-density * 2.4);
            float sampleLight = mix(0.34, 1.0, selfShadow) *
                mix(0.72, 1.0, powder);

            float extinction = density * stepLength * 0.00115;
            float alpha = 1.0 - exp(-extinction);
            float weight = transmittance * alpha;

            accumulatedLight += sampleLight * weight;
            accumulatedWeight += weight;
            transmittance *= (1.0 - alpha);

            if (transmittance < 0.035)
                break;
        }

        t += stepLength;
    }

    float alpha = clamp(1.0 - transmittance, 0.0, 1.0);
    float light = accumulatedWeight > 0.0001
        ? accumulatedLight / accumulatedWeight
        : 1.0;

    return vec2(alpha, light);
}

float CloudShadowFactor(
    vec3 worldPosition,
    vec3 sunDirection,
    float timeSeconds,
    float wind,
    float cloudiness)
{
    if (cloudiness <= 0.01 || sunDirection.y <= 0.035)
        return 1.0;

    const float shadowAltitude = 2200.0;
    float travel = max(
        (shadowAltitude - worldPosition.y) / sunDirection.y,
        0.0);
    vec2 projected = worldPosition.xz + sunDirection.xz * travel;

    float density = CloudCoverageField(projected, timeSeconds, wind, cloudiness);

    float shadowStrength = density * mix(0.18, 0.66, cloudiness);
    return clamp(1.0 - shadowStrength, 0.32, 1.0);
}

#endif
