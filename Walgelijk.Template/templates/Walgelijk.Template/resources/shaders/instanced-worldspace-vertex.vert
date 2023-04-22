#version 460

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec4 color;

layout(location = 3) in vec2 particlePosition;
layout(location = 4) in vec4 particleColor;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = particleColor;
   
   gl_Position = projection * view * model * (vec4(position, 1.0) + vec4(particlePosition.x, particlePosition.y, 0, 0));
}