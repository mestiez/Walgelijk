#version 460

in vec2 uv;
in vec2 screenPos;
in vec4 vertexColor;

out vec4 color;

uniform float time;

void main()
{
    vec4 c = vertexColor;
    c.r = sin(time + screenPos.x * 30) * .5 + .5;
    c.g = 1 - c.r;

    color = c;
}