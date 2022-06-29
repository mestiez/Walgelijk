using System.Linq;

namespace Walgelijk
{
    /// <summary>
    /// Utility struct that provides static text material creation functions
    /// </summary>
    public struct TextMaterial
    {
        public static readonly Shader LegacyShader = new Shader(ShaderDefaults.WorldSpaceVertex, Resources.Load<string>("resources/shaders/legacy-font.frag", true));
        public static readonly Shader SdfShader = new Shader(ShaderDefaults.WorldSpaceVertex, Resources.Load<string>("resources/shaders/sdf-font.frag", true));

        /// <summary>
        /// Create a material for a given font
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        public static Material CreateFor(Font font)
        {
            if (font.Pages == null || font.Pages.Length == 0 || font.Pages.Any(p => p == null))
                return Material.DefaultTextured;

            if (font.Smooth)
                return CreateSDFMaterial(font.Pages);
            else
                return CreateClipMaterial(font.Pages);
        }

        /// <summary>
        /// Create a material for a set of sharp textures
        /// </summary>
        public static Material CreateClipMaterial(IReadableTexture[] pages)
        {
            Material mat = new Material(LegacyShader);

            //TODO meer pages
            mat.SetUniform("mainTex", pages[0]);
            mat.SetUniform("tint", Colors.White);
            return mat;
        }

        /// <summary>
        /// Create a material for a set of SDF textures
        /// </summary>
        public static Material CreateSDFMaterial(IReadableTexture[] pages)
        {
            Material mat = new Material(SdfShader);

            //TODO meer pages
            mat.SetUniform("mainTex", pages[0]);
            mat.SetUniform("thickness", 0.5f);
            mat.SetUniform("softness", 0.0f);
            mat.SetUniform("tint", Colors.White);
            return mat;
        }
    }
}
