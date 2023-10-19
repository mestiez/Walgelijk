namespace Walgelijk;

public static class BatchMaterialCreator
{
    public static readonly Material DefaultInstancedMaterial;
    public static readonly Shader InstancedDefaultShader = new(

//vertex
@"
#version 460

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
    static BatchMaterialCreator()
    {
        DefaultInstancedMaterial = new Material(InstancedDefaultShader);
        DefaultInstancedMaterial.SetUniform(ShaderDefaults.MainTextureUniform, Texture.White);
    }
}