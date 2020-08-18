#version 460

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec4 color;

out vec2 uv;
out vec4 vertexColor;
out vec2 screenPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = color;
   vec4 clip = projection * view * model * vec4(position, 1.0);
   screenPos = clip.xy;
   gl_Position = clip;
}