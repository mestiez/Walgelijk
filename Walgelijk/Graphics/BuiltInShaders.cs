namespace Walgelijk;

/// <summary>
/// Contains all raw built-in shader code used by the engine. These can be changed before a <see cref="Game"/> instance is created
/// </summary>
public static class BuiltInShaders
{
    public readonly static string TexturedFragment =
@"#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    color = vertexColor * texture(mainTex, uv);
}";

    public readonly static string WorldSpaceVertex =
@"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec4 color;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = color;
   gl_Position = projection * view * model * vec4(position, 1.0);
}";

    public readonly static string BatchVertex =
@"
#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 normal;
layout(location = 3) in vec4 color;

layout(location = 4) in mat4 instance_model;
layout(location = 8) in vec4 instance_color;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = color * instance_color;
   gl_Position = projection * view * (model * instance_model) * vec4(position, 1.0);
}";

    public readonly static string BatchFragment =
@"
#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    color = vertexColor * texture(mainTex, uv);
}";

    public static class Text
    {
        public static string BitmapFragment =
@"#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;
uniform vec4 tint;

void main()
{
    vec4 tex = texture(mainTex, uv);
    color = vertexColor * tint;
    color.a = min(tex.r, min(tex.g, min(tex.b, min(tex.a, color.a))));
}
";

        public static string SdfFragment =
@"#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;
uniform float thickness;
uniform float softness;
uniform vec4 tint;

void main()
{
    vec4 tex = texture(mainTex, uv);
    float t = clamp(1-thickness, 0, 1);

    float delta = fwidth(tex.a) / 2 + softness;
    tex.a = smoothstep(t - delta, t + delta , tex.a);
    color = vec4(vertexColor.rgb, vertexColor.a * tex.a) * tint;
}
";

        public static string MsdfFragment =
@"#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;
uniform vec4 tint;
uniform float pxRange = 2;

float median(float r, float g, float b) {
    return max(min(r, g), min(max(r, g), b));
}

float screenPxRange() {
    vec2 unitRange = vec2(pxRange)/vec2(textureSize(mainTex, 0));
    vec2 screenTexSize = vec2(1.0)/fwidth(uv);
    return max(0.5*dot(unitRange, screenTexSize), 1.0);
}

void main() {
    vec3 msd = texture(mainTex, uv).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float screenPxDistance = screenPxRange()*(sd - 0.5);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    color = vec4(vertexColor.rgb, opacity * vertexColor.a) * tint;
}
";
    }
}