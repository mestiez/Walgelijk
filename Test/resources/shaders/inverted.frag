#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    vec4 tex = texture(mainTex, uv);
    tex.rgb = 1 - tex.rgb;
    color = vertexColor * tex;
}