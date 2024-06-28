using System;
using System.IO;

namespace Walgelijk
{
    /// <summary>
    /// Structure that holds the code for a full shader program
    /// </summary>
    public class Shader : IDisposable
    {
        private bool disposed = false;

        /// <summary>
        /// Create a shader from vertex and fragment shader code
        /// </summary>
        public Shader(string vertexShader, string fragmentShader)
        {
            VertexShader = vertexShader;
            FragmentShader = fragmentShader;
        }

        /// <summary>
        /// The vertex shader of the program
        /// </summary>
        public string VertexShader { get; set; }

        /// <summary>
        /// The fragment shader of the program
        /// </summary>
        public string FragmentShader { get; set; }

        /// <summary>
        /// Load shader from files
        /// </summary>
        public static Shader Load(string vertPath, string fragPath)
        {
            string vert = File.ReadAllText(vertPath);
            string frag = File.ReadAllText(fragPath);
            return new Shader(vert, frag);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Game.Main?.Window?.Graphics?.Delete(this);
                GC.SuppressFinalize(this);
            }
            disposed = true;
        }

        /// <summary>
        /// Default shader. Renders vertex colours and textures.
        /// </summary>
        public static readonly Shader Default = new(BuiltInShaders.WorldSpaceVertex, BuiltInShaders.TexturedFragment);
    }
}
