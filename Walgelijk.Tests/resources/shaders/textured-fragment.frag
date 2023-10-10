#version 460

in vec2 uv;
in vec4 vertexColor;
in vec3 normal;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    color = vertexColor * texture(mainTex, uv);
}