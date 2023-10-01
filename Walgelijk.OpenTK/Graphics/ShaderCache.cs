using OpenTK.Graphics.OpenGL4;
using System;

namespace Walgelijk.OpenTK
{
    public class ShaderCache : Cache<Shader, LoadedShader>
    {
        protected override LoadedShader CreateNew(Shader raw)
        {
            CreateShaderProgram(raw, out var vert, out var frag);
            return new LoadedShader
            {
                FragmentShaderHandle = frag,
                VertexShaderHandle = vert
            };
        }

        private static void CreateShaderProgram(Shader shader, out int vert, out int frag)
        {
            vert = GL.CreateShader(ShaderType.VertexShader);
            frag = GL.CreateShader(ShaderType.FragmentShader);

            if (!ShaderCompiler.TryCompileShader(vert, shader.VertexShader))
            {
                GL.DeleteShader(vert);
                GL.DeleteShader(frag);
                throw new Exception("Vertex shader failed to compile");
            }

            if (!ShaderCompiler.TryCompileShader(frag, shader.FragmentShader))
            {
                GL.DeleteShader(vert);
                GL.DeleteShader(frag);
                throw new Exception("Fragment shader failed to compile");
            }
        }

        protected override void DisposeOf(LoadedShader loaded)
        {
            GL.DeleteShader(loaded.VertexShaderHandle);
            GL.DeleteShader(loaded.FragmentShaderHandle);
        }
    }
}
