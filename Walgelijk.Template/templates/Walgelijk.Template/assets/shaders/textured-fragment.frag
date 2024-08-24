// These shaders are not used by the game engine by default, because it actually uses built-in embedded shaders
// accessible from the BuiltInShaders static class. 

//This shader file servers as something to work off of.

#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    color = vertexColor * texture(mainTex, uv);
}