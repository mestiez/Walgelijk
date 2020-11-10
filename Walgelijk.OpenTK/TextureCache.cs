using OpenTK.Graphics.OpenGL;

namespace Walgelijk.OpenTK
{
    public class MaterialTextureCache : Cache<MaterialTexturePair, TextureUnitLink>
    {
        protected override TextureUnitLink CreateNew(MaterialTexturePair raw)
        {
            return new TextureUnitLink(raw.Texture, raw.Material.GetNextTextureUnit());
        }

        protected override void DisposeOf(TextureUnitLink loaded) { }

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
                        loadedTexture  = GPUObjects.TextureCache.Load(v);
                        break;
                    case RenderTexture v:
                        //TODO eigenlijk is dit een beetje raar. RenderTexture moet een texture zijn maar dat is ook vreemd want een RenderTexture is een framebuffer met een texture attachement dus.. niet echt een texture.
                        loadedTexture = GPUObjects.TextureCache.Load(v.Texture);
                        break;
                    default:
                        continue;
                }

                unitLink = Load(new MaterialTexturePair(material, loadedTexture));
                unitLink.Bind();
            }
        }
    }

    public class TextureCache : Cache<IReadableTexture, LoadedTexture>
    {
        protected override LoadedTexture CreateNew(IReadableTexture raw)
        {
            const int componentCount = 4;

            var pixels = raw.GetPixels();
            byte[] data = pixels == null ? null : new byte[pixels.Length * componentCount];

            if (data != null)
            {
                int i = 0;
                foreach (var pixel in pixels)
                {
                    (byte r, byte g, byte b, byte a) = pixel.ToBytes();

                    data[i] = r;
                    data[i + 1] = g;
                    data[i + 2] = b;
                    data[i + 3] = a;

                    i += componentCount;
                }
            }

            GenerateGLTexture(data, raw, out var textureIndex);

            return new LoadedTexture(data, raw.Width, raw.Height, textureIndex);
        }

        private void GenerateGLTexture(byte[] data, IReadableTexture raw, out int textureIndex)
        {
            textureIndex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureIndex);

            var wrap = (int)TypeConverter.Convert(raw.WrapMode);
            var maxFilter = (int)TypeConverter.Convert(raw.FilterMode);
            var mipmapFilter = (int)TextureMinFilter.LinearMipmapLinear;

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrap);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, raw.GenerateMipmaps ? mipmapFilter : maxFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, maxFilter);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, raw.Width, raw.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        protected override void DisposeOf(LoadedTexture loaded)
        {
            GL.DeleteTexture(loaded.Index);
        }
    }
}
