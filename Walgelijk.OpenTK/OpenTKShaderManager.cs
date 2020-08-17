using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Walgelijk.OpenTK
{
    public class OpenTKShaderManager : IShaderManager
    {
        public ShaderCache ShaderCache { get; } = new ShaderCache();

        public void SetUniform(Shader shader, string uniformName, object data)
        {
            var loaded = ShaderCache.Load(shader);
            int prog = loaded.ProgramHandle;
            int loc = loaded.GetUniformLocation(uniformName);

            // Ik haat dit. Ik haat deze hele class.

            switch (data)
            {
                case float v:
                    GL.ProgramUniform1(prog, loc, v);
                    break;
                case double v:
                    GL.ProgramUniform1(prog, loc, v);
                    break;
                case int v:
                    GL.ProgramUniform1(prog, loc, v);
                    break;
                case byte v:
                    GL.ProgramUniform1(prog, loc, v);
                    break;
                case Vector2 v:
                    GL.ProgramUniform2(prog, loc, v.X, v.Y);
                    break;
                case Vector3 v:
                    GL.ProgramUniform3(prog, loc, v.X, v.Y, v.Z);
                    break;
                case Color v:
                    GL.ProgramUniform4(prog, loc, v.R, v.G, v.B, v.A);
                    break;
                case Vector4 v:
                    GL.ProgramUniform4(prog, loc, v.X, v.Y, v.Z, v.W);
                    break;
                case float[] v:
                    GL.ProgramUniform1(prog, loc, v.Length, v);
                    break;
                case double[] v:
                    GL.ProgramUniform1(prog, loc, v.Length, v);
                    break;
                case int[] v:
                    GL.ProgramUniform1(prog, loc, v.Length, v);
                    break;
                //TODO vector arrays
                case Matrix4x4 v:
                    // Ik haat dit ook
                    var typeConverted = new Matrix4(
                        v.M11, v.M12, v.M13, v.M14,
                        v.M21, v.M22, v.M23, v.M24,
                        v.M31, v.M32, v.M33, v.M34,
                        v.M41, v.M42, v.M43, v.M44
                        );
                    GL.ProgramUniformMatrix4(prog, loc, false, ref typeConverted);
                    break;
            }
        }

        public bool TryGetUniform<T>(Shader shader, string uniformName, out T data)
        {
            var loaded = ShaderCache.Load(shader);
            int prog = loaded.ProgramHandle;
            int loc = loaded.GetUniformLocation(uniformName);
            // TODO try get uniform method
            throw new NotImplementedException();
        }
    }
}
