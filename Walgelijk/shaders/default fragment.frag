#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

void main()
{
    color = vertexColor;
}
