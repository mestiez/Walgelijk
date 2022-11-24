using OpenTK.Graphics.OpenGL;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Walgelijk.OpenTK
{
    public class ShaderManager
    {
        public static ShaderManager Instance { get; } = new ShaderManager();

        private readonly float[] matrixBuffer = new float[16];

        public void SetUniform<T>(Material material, string uniformName, T data)
        {
            var loaded = GPUObjects.MaterialCache.Load(material);
            int prog = loaded.ProgramHandle;
            int loc = loaded.GetUniformLocation(uniformName);
            GLUtilities.PrintGLErrors(Game.Main.DevelopmentMode, $"GetUniformLocation failed! Uniform {uniformName} did not accept value {data}");

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
                    {
                        var a = ArrayPool<float>.Shared.Rent(v.Length * 3);
                        int ii = 0;
                        for (int i = 0; i < v.Length; i++)
                        {
                            a[ii++] = v[i].X;
                            a[ii++] = v[i].Y;
                            a[ii++] = v[i].Z;
                        }
                        GL.ProgramUniform3(prog, loc, v.Length, a);
                        ArrayPool<float>.Shared.Return(a);
                    }
                    break;
                //TODO vector arrays
                case Matrix4x4 v:
                    SetMatrixBuffer(v);
                    GL.ProgramUniformMatrix4(prog, loc, 1, false, matrixBuffer);
                    break;
                case Matrix4x4[] v:
                    {
                        var a = ArrayPool<float>.Shared.Rent(v.Length * 16);
                        int ii = 0;
                        for (int i = 0; i < v.Length; i++)
                        {
                            var vv = v[i];
                            a[ii++] = vv.M11;
                            a[ii++] = vv.M12;
                            a[ii++] = vv.M13;
                            a[ii++] = vv.M14;

                            a[ii++] = vv.M21;
                            a[ii++] = vv.M22;
                            a[ii++] = vv.M23;
                            a[ii++] = vv.M24;

                            a[ii++] = vv.M31;
                            a[ii++] = vv.M32;
                            a[ii++] = vv.M33;
                            a[ii++] = vv.M34;

                            a[ii++] = vv.M41;
                            a[ii++] = vv.M42;
                            a[ii++] = vv.M43;
                            a[ii++] = vv.M44;
                        }
                        GL.ProgramUniformMatrix4(prog, loc, v.Length, false, a);
                        ArrayPool<float>.Shared.Return(a);
                    }
                    break;
                default:
                    Logger.Error("Invalid uniform datatype!");
                    break;
            }

            GLUtilities.PrintGLErrors(Game.Main.DevelopmentMode);

            //switch (GL.GetError())
            //{
            //    case not ErrorCode.NoError:
            //        Logger.Error("Failed to set uniform...");
            //        break;
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetMatrixBuffer(Matrix4x4 v)
        {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForceLoadTexture(IReadableTexture texture)
        {
            GPUObjects.TextureCache.Load(texture);
        }
    }
}
