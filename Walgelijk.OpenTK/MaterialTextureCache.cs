namespace Walgelijk.OpenTK
{
    public class MaterialTextureCache : Cache<MaterialTexturePair, TextureUnitLink>
    {
        protected override TextureUnitLink CreateNew(MaterialTexturePair raw)
        {
            return new TextureUnitLink(raw.Texture, raw.Material.GetNextTextureUnit());
        }

        protected override void DisposeOf(TextureUnitLink loaded)
        {
            
        }

        internal void ActivateTexturesFor(LoadedMaterial material)
        {
            var allUniforms = material.Material.GetAllUniforms();

            LoadedTexture loadedTexture;
            TextureUnitLink unitLink;

            foreach (var pair in allUniforms)
            {
                switch (pair.Value)
                {
                    case IReadableTexture v:
                        loadedTexture = GPUObjects.TextureCache.Load(v);
                        break;
                    default:
                        continue;
                }

                unitLink = Load(new MaterialTexturePair(material, loadedTexture));
                unitLink.Bind();
            }
        }
    }
}
