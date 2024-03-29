﻿#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec4 color;

out vec2 uv;
out vec4 vertexColor;
out vec2 worldPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform float time;

void main()
{
   uv = texcoord;
   vertexColor = color;
   worldPos = (model * vec4(position, 1.0)).xy;
   vec4 pos = projection * view * model * vec4(position, 1.0);
   pos.x += sin(time * .1 + worldPos.y) * 0.06 * projection[0][0];
   gl_Position = pos;
}