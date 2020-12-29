#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    const float intensity = 0.01;

    float i = 1 + intensity;
    float r = texture(mainTex, uv).r;
    float g = texture(mainTex, uv * i).g;
    float b = texture(mainTex, uv * (1/i)).b;

    color = vertexColor * vec4(r,g,b,1);
}