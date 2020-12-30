#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    const float intensity = 0.05;

    float i = 1 + intensity;
    vec2 cUv = uv - .5;

    vec4 original = texture(mainTex, uv);

    float r = original.r;
    float g = texture(mainTex, cUv * i+ .5).g;
    float b = texture(mainTex, cUv * (1/i) + .5).b;

    color = vertexColor * vec4(r,g,b, max(r,max(g,max(b,original.a))));
}