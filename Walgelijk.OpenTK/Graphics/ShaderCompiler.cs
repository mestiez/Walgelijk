using OpenTK.Graphics.OpenGL;

namespace Walgelijk.OpenTK
{
    public struct ShaderCompiler
    {
        public static bool TryCompileShader(int index, string code)
        {
            CompileShader(index, code);

            GL.GetShaderInfoLog(index, out string info);
            GL.GetShader(index, ShaderParameter.CompileStatus, out int compilationResult);

            bool compilationFailed = compilationResult == (int)All.False;

            if (compilationFailed)
            {
                Logger.Error($"Shader {index} error:\n{info}", nameof(ShaderCompiler));
                GL.DeleteShader(index);
                return false;
            }
            else
                return true;
        }

        private static void CompileShader(int index, string code)
        {
            GL.ShaderSource(index, code);
            GL.CompileShader(index);
        }
    }
}
