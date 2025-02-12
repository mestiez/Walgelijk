using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

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
            // we have no ownership over any of the assets in the structure
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

        internal void UnloadMaterial(LoadedMaterial loaded)
        {
            Queue<MaterialTexturePair> toUnload = [];

            foreach (var item in Loaded)
                if (item.Key.Material == loaded)
                    toUnload.Enqueue(item.Key);

            while (toUnload.TryDequeue(out var k))
                Unload(k);
        }

        internal void UnloadTexture(LoadedTexture loaded)
        {
            Queue<MaterialTexturePair> toUnload = [];

            foreach (var item in Loaded)
                if (item.Key.Texture == loaded)
                    toUnload.Enqueue(item.Key);

            while (toUnload.TryDequeue(out var k))
                Unload(k);
        }
    }
}
