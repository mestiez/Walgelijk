using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK
{
    public class LoadedShader
    {
        public int ProgramHandle { get; private set; }

        public Shader Shader { get; private set; }

        private Dictionary<string, int> uniformLocations = new Dictionary<string, int>();

        public LoadedShader(Shader shader)
        {
            CreateShaderProgram(
                shader, 
                out int vertexShaderIndex, 
                out int fragmentShaderIndex, 
                out int programIndex);

            GL.AttachShader(programIndex, vertexShaderIndex);
            GL.AttachShader(programIndex, fragmentShaderIndex);

            GL.LinkProgram(programIndex);
            GL.GetProgram(programIndex, GetProgramParameterName.LinkStatus, out int linkStatus);

            bool linkingFailed = linkStatus == (int)All.False;

            if (linkingFailed)
            {
                GL.DeleteProgram(programIndex);
                GL.DeleteShader(vertexShaderIndex);
                GL.DeleteShader(fragmentShaderIndex);
                throw new Exception("Shader program failed to link");
            }

            GL.ValidateProgram(programIndex);

            GL.DeleteShader(vertexShaderIndex);
            GL.DeleteShader(fragmentShaderIndex);

            GL.DetachShader(programIndex, vertexShaderIndex);
            GL.DetachShader(programIndex, fragmentShaderIndex);

            Shader = shader;
            ProgramHandle = programIndex;
        }

        private static void CreateShaderProgram(Shader shader, out int vert, out int frag, out int prog)
        {
            vert = GL.CreateShader(ShaderType.VertexShader);
            frag = GL.CreateShader(ShaderType.FragmentShader);
            prog = GL.CreateProgram();
            if (!ShaderCompiler.TryCompileShader(vert, shader.VertexShader)) throw new Exception("Vertex shader failed to compile");
            if (!ShaderCompiler.TryCompileShader(frag, shader.FragmentShader)) throw new Exception("Fragment shader failed to compile");
        }

        public int GetUniformLocation(string name)
        {
            if (uniformLocations.TryGetValue(name, out int loc))
                return loc;

            loc = GL.GetUniformLocation(ProgramHandle, name);

            uniformLocations.Add(name, loc);
            return loc;
        }
    }
}
