#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    float r = texture(mainTex, uv).r;
    float g = texture(mainTex, uv * 1.01).g;
    float b = texture(mainTex, uv * 0.99).b;

    color = vertexColor * vec4(r,g,b,1);
}