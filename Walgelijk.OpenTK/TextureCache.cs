using OpenTK.Graphics.OpenGL;

namespace Walgelijk.OpenTK
{
    public class TextureCache : Cache<IReadableTexture, LoadedTexture>
    {
        protected override LoadedTexture CreateNew(IReadableTexture raw)
        {
            const int componentCount = 4;

            var pixels = raw.GetPixels();
            byte[] data = pixels.HasValue ? new byte[pixels.Value.Length * componentCount] : null;

            if (pixels.HasValue)
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
