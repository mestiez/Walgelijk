using OpenTK.Graphics.OpenGL4;
using System;

namespace Walgelijk.OpenTK
{
    public class ShaderCache : Cache<Shader, LoadedShader>
    {
        public override LoadedShader Load(Shader raw)
        {
            var loaded = base.Load(raw);

            if (raw.NeedsUpdate)
            {
                UploadShader(raw, loaded);
                raw.NeedsUpdate = false;
            }

            return loaded;
        }

        private static void UploadShader(Shader raw, LoadedShader loaded)
        {
            if (!ShaderCompiler.TryCompileShader(loaded.VertexShaderHandle, raw.VertexShader))
            {
                GL.DeleteShader(loaded.VertexShaderHandle);
                GL.DeleteShader(loaded.FragmentShaderHandle);
                throw new Exception("Vertex shader failed to compile");
            }

            if (!ShaderCompiler.TryCompileShader(loaded.FragmentShaderHandle, raw.FragmentShader))
            {
                GL.DeleteShader(loaded.VertexShaderHandle);
                GL.DeleteShader(loaded.FragmentShaderHandle);
                throw new Exception("Fragment shader failed to compile");
            }
        }

        protected override LoadedShader CreateNew(Shader raw)
        {
            var vert = GL.CreateShader(ShaderType.VertexShader);
            var frag = GL.CreateShader(ShaderType.FragmentShader);
            var loaded = new LoadedShader
            {
                FragmentShaderHandle = frag,
                VertexShaderHandle = vert
            };

            UploadShader(raw, loaded);

            return loaded;
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
