#version 460

in vec2 uv;
in vec2 worldPos;
in vec4 vertexColor;

out vec4 color;

uniform float time;

uniform sampler2D texture1;
uniform sampler2D texture2;

void main()
{
    float p = sin(time*.25 + worldPos.x * 2) * .5 + .5;

    vec4 c = vertexColor;

    vec4 tex1 = texture(texture1, uv);
    vec4 tex2 = texture(texture2, uv);

    color = c * mix(tex1, tex2, smoothstep(0.10,0.60, p));
}