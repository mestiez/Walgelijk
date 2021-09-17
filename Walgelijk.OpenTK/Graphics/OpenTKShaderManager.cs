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
    public class ShaderManager
    {
        public static ShaderManager Instance { get; } = new ShaderManager();

        private readonly float[] matrixBuffer = new float[16];

        //TODO dit is geen goede oplossing
        private readonly MatrixArrayCache matrixArrayCache = new();
        private readonly Vector3ArrayCache v3ArrayCache = new();

        public void SetUniform(Material material, string uniformName, object data)
        {
            var loaded = GPUObjects.MaterialCache.Load(material);
            int prog = loaded.ProgramHandle;
            int loc = loaded.GetUniformLocation(uniformName);
            GLUtilities.PrintGLErrors(true, "GetUniformLocation failed! ");

            // Ik haat dit. Ik haat deze hele class.
            LoadedTexture loadedTexture;
            TextureUnitLink unitLink;

            switch (data)
            {
                case float v:
                    GL.ProgramUniform1(prog, loc, v);
                    break;
                case IReadableTexture v:
                    loadedTexture = GPUObjects.TextureCache.Load(v);
                    unitLink = GPUObjects.MaterialTextureCache.Load(new MaterialTexturePair(loaded, loadedTexture, loc));
                    var textureUnit = TypeConverter.Convert(unitLink.Unit);
                    GL.ProgramUniform1(prog, loc, 1, ref textureUnit);
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
                case Vector3[] v:
                    var arr = v3ArrayCache.GetOrUpdate(v);
                    GL.ProgramUniform3(prog, loc, v.Length, arr);
                    GLUtilities.PrintGLErrors(true, $"Cannot upload Vector3 array for some reason: Array is {(string.Join(", ", arr) ?? "NULL")}");
                    break;
                //TODO vector arrays
                case Matrix4x4 v:
                    SetMatrixBuffer(v);
                    GL.ProgramUniformMatrix4(prog, loc, 1, false, matrixBuffer);
                    break;
                case Matrix4x4[] v:
                    var a = matrixArrayCache.Load(v);
                    GL.ProgramUniformMatrix4(prog, loc, v.Length, false, a);
                    break;
                default:
                    Logger.Error("Invalid uniform datatype!");
                    break;
            }

            GLUtilities.PrintGLErrors(true);

            //switch (GL.GetError())
            //{
            //    case not ErrorCode.NoError:
            //        Logger.Error("Failed to set uniform...");
            //        break;
            //}
        }

        private void SetMatrixBuffer(Matrix4x4 v)
        {
            //Ja dankjewel System.Numerics voor deze shitshow. hartelijk bedankt
            matrixBuffer[0] = v.M11;
            matrixBuffer[1] = v.M12;
            matrixBuffer[2] = v.M13;
            matrixBuffer[3] = v.M14;

            matrixBuffer[4] = v.M21;
            matrixBuffer[5] = v.M22;
            matrixBuffer[6] = v.M23;
            matrixBuffer[7] = v.M24;

            matrixBuffer[8] = v.M31;
            matrixBuffer[9] = v.M32;
            matrixBuffer[10] = v.M33;
            matrixBuffer[11] = v.M34;

            matrixBuffer[12] = v.M41;
            matrixBuffer[13] = v.M42;
            matrixBuffer[14] = v.M43;
            matrixBuffer[15] = v.M44;
        }

        public void ForceLoadTexture(IReadableTexture texture)
        {
            GPUObjects.TextureCache.Load(texture);
        }
    }

    internal class Vector3ArrayCache : Cache<Vector3[], float[]>
    {
        private const int length = 3;

        protected override float[] CreateNew(Vector3[] raw)
        {
            var array = new float[length * raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                var e = raw[i];
                int index = length * i;
                array[index + 0] = e.X;
                array[index + 1] = e.Y;
                array[index + 2] = e.Z;
            }
            return array;
        }

        public float[] GetOrUpdate(Vector3[] raw)
        {
            var array = Load(raw);
            for (int i = 0; i < raw.Length; i++)
            {
                var e = raw[i];
                int index = length * i;
                array[index + 0] = e.X;
                array[index + 1] = e.Y;
                array[index + 2] = e.Z;
            }
            return array;
        }

        protected override void DisposeOf(float[] loaded)
        {
        }
    }

    internal class MatrixArrayCache : Cache<Matrix4x4[], float[]>
    {
        private const int MatrixLength = 16;

        protected override float[] CreateNew(Matrix4x4[] raw)
        {
            var array = new float[MatrixLength * raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                var v = raw[i];
                int index = i * MatrixLength;

                array[index + 0] = v.M11;
                array[index + 1] = v.M12;
                array[index + 2] = v.M13;
                array[index + 3] = v.M14;

                array[index + 4] = v.M21;
                array[index + 5] = v.M22;
                array[index + 6] = v.M23;
                array[index + 7] = v.M24;

                array[index + 8] = v.M31;
                array[index + 9] = v.M32;
                array[index + 10] = v.M33;
                array[index + 11] = v.M34;

                array[index + 12] = v.M41;
                array[index + 13] = v.M42;
                array[index + 14] = v.M43;
                array[index + 15] = v.M44;
            }

            return array;
        }

        protected override void DisposeOf(float[] loaded)
        {

        }
    }
}
