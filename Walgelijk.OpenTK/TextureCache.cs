using OpenTK.Graphics.OpenGL;
using System;

namespace Walgelijk.OpenTK
{
    public struct MaterialTexturePair
    {
        public LoadedMaterial Material;
        public IReadableTexture Texture;

        public MaterialTexturePair(LoadedMaterial material, IReadableTexture texture)
        {
            Material = material;
            Texture = texture;
        }
    }

    public class TextureCache : Cache<MaterialTexturePair, LoadedTexture>
    {
        public void ActivateTexturesFor(LoadedMaterial material)
        {
            //TODO dit moet sneller??
            var allUniforms = material.Material.GetAllUniforms();

            foreach (var pair in allUniforms)
            {
                if (pair.Value is IReadableTexture texture)
                {
                    var loaded = Load(new MaterialTexturePair(material, texture));
                    loaded.Bind();
                }
            }
        }

        protected override LoadedTexture CreateNew(MaterialTexturePair pair)
        {
            const int componentCount = 4;

            var raw = pair.Texture;

            var pixels = raw.GetPixels();
            byte[] data = new byte[pixels.Length * componentCount];

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

            GenerateGLTexture(data, pair, out var textureIndex, out var textureUnit);

            return new LoadedTexture(data, raw.Width, raw.Height, textureUnit, textureIndex);
        }

        private void GenerateGLTexture(byte[] data, MaterialTexturePair pair, out int textureIndex, out TextureUnit textureUnit)
        {
            var raw = pair.Texture;

            textureUnit = pair.Material.GetNextTextureUnit();
            textureIndex = GL.GenTexture();
            GL.ActiveTexture(textureUnit);
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
            throw new NotImplementedException();
        }
    }
}
