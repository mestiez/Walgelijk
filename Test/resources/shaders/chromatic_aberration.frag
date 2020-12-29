#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    const float intensity = 0.11;

    float i = 1 + intensity;
    vec2 cUv = uv - .5;

    float r = texture(mainTex, cUv+ .5).r;
    float g = texture(mainTex, cUv * i+ .5).g;
    float b = texture(mainTex, cUv * (1/i) + .5).b;

    color = vertexColor * vec4(r,g,b,1);
}