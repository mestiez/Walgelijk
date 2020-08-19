namespace Walgelijk
{
    /// <summary>
    /// Useful constants for shader related business 
    /// </summary>
    public struct ShaderConstants
    {
        /// <summary>
        /// Default fragment shader code
        /// </summary>
        public const string DefaultFragment = @"#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    color = vertexColor * texture(mainTex, uv);
}";
        /// <summary>
        /// Default vertex shader code
        /// </summary>
        public const string DefaultVertex = @"#version 460

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec4 color;

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

        /// <summary>
        /// Projection matrix uniform name
        /// </summary>
        public const string ProjectionMatrixUniform = "projection";
        /// <summary>
        /// View matrix uniform name
        /// </summary>
        public const string ViewMatrixUniform = "view";
        /// <summary>
        /// Model matrix uniform name
        /// </summary>
        public const string ModelMatrixUniform = "model";     
        /// <summary>
        /// Main texture uniform name
        /// </summary>
        public const string MainTextureUniform = "mainTex";
    }
}
