#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    vec2 c = uv;
    c.x += sin(c.y * 40) * 0.01f;
    color = vertexColor * texture(mainTex, c);
    color.rgb = vec3(1) - color.rgb;
}