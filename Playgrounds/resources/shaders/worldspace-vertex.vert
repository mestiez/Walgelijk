#version 460

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec4 color;

out vec2 uv;
out vec4 vertexColor;
out vec3 normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = color;
   normal = in_normal;
   gl_Position = projection * view * model * vec4(position, 1.0);
}