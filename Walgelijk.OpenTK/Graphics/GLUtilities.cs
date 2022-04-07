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
