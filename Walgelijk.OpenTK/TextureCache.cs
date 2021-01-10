using OpenTK.Graphics.OpenGL;
using System.Collections.Immutable;

namespace Walgelijk.OpenTK
{
    public class TextureCache : Cache<IReadableTexture, LoadedTexture>
    {
        protected override LoadedTexture CreateNew(IReadableTexture raw)
        {
            const int componentCount = 4;

            GenerateGLTexture(raw, out var textureIndex);

            var pixels = raw.GetPixels();

            if (pixels.HasValue)
            {
                //TODO dit is een beetje lelijke code
                if (raw.HDR)
                    WriteHDRData(raw, componentCount, pixels);
                else
                    WriteLDRData(raw, componentCount, pixels);
            }
            else
            {
                if (raw.HDR)
                    SetTextureData((float[])null, raw);
                else
                    SetTextureData((byte[])null, raw);
            }

            if (raw.GenerateMipmaps)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new LoadedTexture(raw.Width, raw.Height, textureIndex);
        }

        private void WriteLDRData(IReadableTexture raw, int componentCount, ImmutableArray<Color>? pixels)
        {
            var data = new byte[raw.Width * raw.Height * componentCount];
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
            SetTextureData(data, raw);
        }

        private void WriteHDRData(IReadableTexture raw, int componentCount, ImmutableArray<Color>? pixels)
        {
            var data = new float[raw.Width * raw.Height * componentCount];
            int i = 0;
            foreach (var pixel in pixels)
            {
                data[i] = pixel.R;
                data[i + 1] = pixel.G;
                data[i + 2] = pixel.B;
                data[i + 3] = pixel.A;

                i += componentCount;
            }
            SetTextureData(data, raw);
        }

        private void GenerateGLTexture(IReadableTexture raw, out int textureIndex)
        {
            textureIndex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureIndex);

            var wrap = (int)TypeConverter.Convert(raw.WrapMode);
            var maxFilter = (int)TypeConverter.Convert(raw.FilterMode);
            var mipmapFilter = (int)TextureMinFilter.LinearMipmapLinear;

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrap);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, raw.GenerateMipmaps ? mipmapFilter : maxFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, maxFilter); ;

        }

        private void SetTextureData(byte[] data, IReadableTexture raw)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, raw.Width, raw.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        private void SetTextureData(float[] data, IReadableTexture raw)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, raw.Width, raw.Height, 0, PixelFormat.Rgba, PixelType.Float, data);
        }

        protected override void DisposeOf(LoadedTexture loaded)
        {
            GL.DeleteTexture(loaded.Index);
        }
    }
}
