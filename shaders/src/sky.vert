#version 450
layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

layout(location = 0) out vec3 fsin_WorldDirection;

void main()
{
    vec2 positions[3] = vec2[](
        vec2(-1.0, -1.0),
        vec2( 3.0, -1.0),
        vec2(-1.0,  3.0)
    );

    vec2 ndc = positions[gl_VertexIndex];
    gl_Position = vec4(ndc, 1.0, 1.0);

    vec4 viewPoint = inverse(Projection) * vec4(ndc, 1.0, 1.0);
    vec3 viewDirection = normalize(viewPoint.xyz / max(abs(viewPoint.w), 0.00001));
    mat3 inverseViewRotation = transpose(mat3(View));
    fsin_WorldDirection = normalize(inverseViewRotation * viewDirection);
}
