using System.Linq;

namespace Walgelijk
{
    /// <summary>
    /// Utility struct that provides static text material creation functions
    /// </summary>
    public struct TextMaterial
    {
        /// <summary>
        /// Create a material for a given font
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        public static Material CreateFor(Font font)
        {
            if (font.Pages.Length == 0 || font.Pages.Any(p => p == null))
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
            string vert = ShaderDefaults.WorldSpaceVertex;
            string frag = Resources.Load<string>(Game.Main.ExecutableDirectory + "resources\\shaders\\legacy-font.frag", true);
            Material mat = new Material(new Shader(vert, frag));

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
            string vert = ShaderDefaults.WorldSpaceVertex;
            string frag = Resources.Load<string>(Game.Main.ExecutableDirectory + "resources\\shaders\\sdf-font.frag", true);
            Material mat = new Material(new Shader(vert, frag));

            //TODO meer pages
            mat.SetUniform("mainTex", pages[0]);
            mat.SetUniform("thickness", 0.5f);
            mat.SetUniform("softness", 0.0f);
            mat.SetUniform("tint", Colors.White);
            return mat;
        }
    }
}
