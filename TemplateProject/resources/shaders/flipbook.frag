#version 450

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

uniform float rows = 1;
uniform float columns = 1;
uniform float progress = 0;
uniform vec4 tint = vec4(1,1,1,1);

float getColumn(float i)
{
    return floor(mod((i), max(columns, 1)));
}

float getRow(float i)
{
    return floor(i / max(columns, 1));
}

void main()
{
    float width = max(columns, 1);
    float height = max(rows, 1);
    float i = floor(width * height * progress);
    vec2 offset = vec2(getColumn(i), getRow(i));
    vec2 newUv = vec2((uv.x + offset.x) / width, (uv.y + offset.y) / height);
    color = vertexColor * tint * texture(mainTex,  newUv);
    // color = vertexColor * texture(mainTex, vec2((uv.x + i) / 4, uv.y));
}