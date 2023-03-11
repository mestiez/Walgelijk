namespace Walgelijk.ParticleSystem
{
    internal static class ParticleMaterialInitialiser
    {
        public static readonly Shader DefaultShader = new(

//vertex
@"
#version 460

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec4 color;

layout(location = 3) in mat4 particleModel;
layout(location = 7) in vec4 particleColor;
layout(location = 8) in vec4 particleUvOffset;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = (texcoord * particleUvOffset.zw) + particleUvOffset.xy;
   vertexColor = color * particleColor;
   
   gl_Position = projection * view * (model * particleModel) * vec4(position, 1.0);
}",

//fragment
    @"
#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    color = vertexColor * texture(mainTex, uv);
}"
    );

        public static Material CreateDefaultMaterial()
        {
            var mat = new Material(DefaultShader);
            mat.SetUniform(ShaderDefaults.MainTextureUniform, Texture.White);
            return mat;
        }
    }
}
