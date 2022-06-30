using OpenTK.Graphics.OpenGL;
using System;

namespace Walgelijk.OpenTK
{
    internal struct GLUtilities
    {
        private static int MaximumTextureUnit = -1;

        public static int GetMaximumAmountOfTextureUnits()
        {
            if (MaximumTextureUnit == -1)
                GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out MaximumTextureUnit);
            return MaximumTextureUnit;
        }

        public static void SetBlendMode(BlendMode mode)
        {
            switch (mode)
            {
                case BlendMode.Negate:
                    GL.BlendFuncSeparate(
                        BlendingFactorSrc.One,
                        BlendingFactorDest.One,
                        BlendingFactorSrc.One, 
                        BlendingFactorDest.One);

                    GL.BlendEquationSeparate(
                        BlendEquationMode.FuncSubtract, 
                        BlendEquationMode.FuncAdd);
                    break;
                case BlendMode.Lighten:
                    GL.BlendFuncSeparate(
                        BlendingFactorSrc.SrcAlpha,
                        BlendingFactorDest.OneMinusSrcAlpha,
                        BlendingFactorSrc.SrcAlpha, 
                        BlendingFactorDest.One);

                    GL.BlendEquationSeparate(
                        BlendEquationMode.Max, 
                        BlendEquationMode.FuncAdd);
                    break;
                case BlendMode.Multiply:
                    GL.BlendFuncSeparate(
                        BlendingFactorSrc.DstColor,
                        BlendingFactorDest.OneMinusSrcAlpha,
                        BlendingFactorSrc.SrcAlpha, 
                        BlendingFactorDest.One);

                    GL.BlendEquationSeparate(
                        BlendEquationMode.FuncAdd, 
                        BlendEquationMode.FuncAdd);
                    break;
                case BlendMode.Screen:
                    GL.BlendFuncSeparate(
                        BlendingFactorSrc.DstAlpha,
                        BlendingFactorDest.OneMinusSrcColor,
                        BlendingFactorSrc.SrcAlpha, 
                        BlendingFactorDest.One);

                    GL.BlendEquationSeparate(
                        BlendEquationMode.FuncAdd, 
                        BlendEquationMode.FuncAdd);
                    break;
                case BlendMode.Addition:
                    GL.BlendFuncSeparate(
                        BlendingFactorSrc.SrcAlpha,
                        BlendingFactorDest.One,
                        BlendingFactorSrc.SrcAlpha, 
                        BlendingFactorDest.One);

                    GL.BlendEquationSeparate(
                        BlendEquationMode.FuncAdd, 
                        BlendEquationMode.FuncAdd);
                    break;
                default:
                case BlendMode.AlphaBlend:
                    GL.BlendFuncSeparate(
                        BlendingFactorSrc.SrcAlpha, 
                        BlendingFactorDest.OneMinusSrcAlpha, 
                        BlendingFactorSrc.SrcAlpha, 
                        BlendingFactorDest.One);
                    GL.BlendEquationSeparate(
                        BlendEquationMode.FuncAdd, 
                        BlendEquationMode.FuncAdd);
                    break;
            }
        }

        public static void PrintGLErrors(bool throwException = false, string exceptionPrepend = null)
        {
            while (true)
            {
                var e = GL.GetError();
                if (e == ErrorCode.NoError)
                    break;
                Logger.Error("GL Error: " + e.ToString(), nameof(OpenTKWindow));
                if (throwException)
                    throw new GraphicsException((exceptionPrepend ?? string.Empty) + e.ToString());
            }
        }
    }

    public class GraphicsException : Exception
    {
        public GraphicsException(string error) : base(error)
        {
        }
    }
}
