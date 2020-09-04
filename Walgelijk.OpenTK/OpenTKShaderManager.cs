using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing.Text;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Walgelijk.OpenTK
{
    public class OpenTKShaderManager : IShaderManager
    {
        public MaterialCache MaterialCache { get; } = new MaterialCache();
        public TextureCache TextureCache { get; } = new TextureCache();

        private readonly float[] matrixBuffer = new float[16];

        public void SetUniform(Material material, string uniformName, object data)
        {
            var loaded = MaterialCache.Load(material);
            int prog = loaded.ProgramHandle;
            int loc = loaded.GetUniformLocation(uniformName);

            // Ik haat dit. Ik haat deze hele class.

            switch (data)
            {
                case IReadableTexture v:
                    var loadedTexture = TextureCache.Load(new MaterialTexturePair(loaded, v));
                    GL.ProgramUniform1(prog, loc, TypeConverter.Convert(loadedTexture.TextureUnit));
                    break;                
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
                    SetMatrixBuffer(v);
                    GL.ProgramUniformMatrix4(prog, loc, 1, false, matrixBuffer);
                    break;
            }
        }

        private void SetMatrixBuffer(Matrix4x4 v)
        {
            //Ja dankjewel System.Numerics voor deze shitshow. hartelijk bedankt
            matrixBuffer[0]  = v.M11;
            matrixBuffer[1]  = v.M12;
            matrixBuffer[2]  = v.M13;
            matrixBuffer[3]  = v.M14;
                             
            matrixBuffer[4]  = v.M21;
            matrixBuffer[5]  = v.M22;
            matrixBuffer[6]  = v.M23;
            matrixBuffer[7]  = v.M24;
                             
            matrixBuffer[8]  = v.M31;
            matrixBuffer[9]  = v.M32;
            matrixBuffer[10] = v.M33;
            matrixBuffer[11] = v.M34;

            matrixBuffer[12] = v.M41;
            matrixBuffer[13] = v.M42;
            matrixBuffer[14] = v.M43;
            matrixBuffer[15] = v.M44;
        }

        public bool TryDownloadUniform<T>(Material material, string uniformName, out T data)
        {
            var loaded = MaterialCache.Load(material);
            int prog = loaded.ProgramHandle;
            int loc = loaded.GetUniformLocation(uniformName);
            throw new NotImplementedException();
        }
    }
}
