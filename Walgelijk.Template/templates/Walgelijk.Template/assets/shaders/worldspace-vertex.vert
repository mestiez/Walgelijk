// These shaders are not used by the game engine by default, because it actually uses built-in embedded shaders
// accessible from the BuiltInShaders static class. 

//This shader file servers as something to work off of.

#version 330 core

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_texcoord;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec4 in_color;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = in_texcoord;
   vertexColor = in_color;
   gl_Position = projection * view * model * vec4(in_position, 1.0);
}