namespace Walgelijk.Prism;

public static class BuiltInShaders
{
    public static Shader Pbr = new Shader(
        @"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec2 texcoord2;
layout(location = 3) in vec3 in_normal;
layout(location = 4) in vec3 in_tangent;
layout(location = 5) in vec3 in_biTangent;
layout(location = 6) in vec4 color;

out vec2 uv;
out vec2 uv2;
out vec4 vertexColor;
out vec3 normal;
out vec3 tangent;
out vec3 biTangent;
out vec4 worldPosition;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   uv2 = texcoord2;
   vertexColor = color;
   normal = (model * vec4(in_normal, 1)).xyz;
   tangent = (model * vec4(in_tangent, 1)).xyz;
   biTangent = (model * vec4(in_biTangent, 1)).xyz;

   worldPosition = model * vec4(position, 1.0);

   gl_Position = projection * view * worldPosition;
}",
        @"#version 330 core

in vec2 uv;
in vec4 vertexColor;
in vec3 normal;
in vec3 tangent;
in vec3 biTangent;
in vec4 worldPosition;

out vec4 color;

uniform sampler2D albedoMap;
uniform sampler2D roughnessMap;
uniform sampler2D metallicMap;
uniform sampler2D normalMap;
uniform sampler2D world; // Equirectangular world map
uniform vec4 tint = vec4(1, 1, 1, 1);

uniform vec3 sunDir = vec3(0, -1, 0);

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

const float PI = 3.14159265359;

void main() {
	//color = texture(albedoMap, mod(worldPosition.xy, 1)) * vertexColor;
	color = texture(albedoMap, uv) * vertexColor;
	if (color.a < 0.5)
		discard;
	color.a = 1;
}
");
}