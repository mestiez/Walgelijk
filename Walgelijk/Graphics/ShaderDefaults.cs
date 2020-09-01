namespace Walgelijk
{
    /// <summary>
    /// Useful values for shader related business 
    /// </summary>
    public struct ShaderDefaults
    {
        /// <summary>
        /// Default fragment shader code
        /// </summary>
        public static readonly string TexturedFragment = Resources.Load<string>("shaders\\textured-fragment.frag");  
        /// <summary>
        /// Default vertex shader code
        /// </summary>
        public static readonly string WorldSpaceVertex = Resources.Load<string>("shaders\\worldspace-vertex.vert");        

        /// <summary>
        /// Projection matrix uniform name
        /// </summary>
        public const string ProjectionMatrixUniform = "projection";
        /// <summary>
        /// View matrix uniform name
        /// </summary>
        public const string ViewMatrixUniform = "view";
        /// <summary>
        /// Model matrix uniform name
        /// </summary>
        public const string ModelMatrixUniform = "model";     
        /// <summary>
        /// Main texture uniform name
        /// </summary>
        public const string MainTextureUniform = "mainTex";
    }
}
