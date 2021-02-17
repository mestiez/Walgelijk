namespace Walgelijk
{
    /// <summary>
    /// A basic shader post processor. It applies a material to the entire image
    /// </summary>
    public class ShaderPostProcessor : IPostProcessingEffect
    {
        /// <summary>
        /// The material to apply
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Name of the main texture uniform
        /// </summary>
        public string TextureUniform { get; set; } = ShaderDefaults.MainTextureUniform;

        /// <summary>
        /// Construct a post processor with the given material
        /// </summary>
        public ShaderPostProcessor(Material material)
        {
            Material = material;
        }

        /// <summary>
        /// Construct a post processor with a new material and shader, generated from the given fragment shader code
        /// </summary>
        public ShaderPostProcessor(string fragmentShader)
        {
            Material = new Material(new Shader(ShaderDefaults.WorldSpaceVertex, fragmentShader));
        }

        /// <summary>
        /// Blits the source texture to the destination texture using <see cref="Material"/>
        /// </summary>
        public void Process(RenderTexture src, RenderTexture dst, IGraphics graphics, Scene scene)
        {
            graphics.BlitFullscreenQuad(src, dst, Material, TextureUniform);
        }
    }
}
