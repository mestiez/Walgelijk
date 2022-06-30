namespace Walgelijk.OpenTK
{
    public class MaterialTextureCache : Cache<MaterialTexturePair, TextureUnitLink>
    {
        protected override TextureUnitLink CreateNew(MaterialTexturePair raw)
        {
            return new TextureUnitLink(raw.Texture, raw.Material.GetTextureUnitForUniform(raw.UniformLocation));
        }

        protected override void DisposeOf(TextureUnitLink loaded)
        {

        }

        internal void ActivateTexturesFor(LoadedMaterial material)
        {
            LoadedTexture loadedTexture;
            TextureUnitLink unitLink;

            foreach (var pair in material.Material.InternalUniforms.Textures)
            {
                loadedTexture = GPUObjects.TextureCache.Load(pair.Value);
                unitLink = Load(new MaterialTexturePair(material, loadedTexture, material.GetUniformLocation(pair.Key)));
                unitLink.Bind();
            }
        }
    }
}
