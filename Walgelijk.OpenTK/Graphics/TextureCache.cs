using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Immutable;

namespace Walgelijk.OpenTK
{
    internal class FloatArrayCache : Cache<int, float[]>
    {
        protected override float[] CreateNew(int raw)
        {
            return new float[raw];
        }

        protected override void DisposeOf(float[] loaded) { }
    }

    internal class ByteArrayCache : Cache<int, byte[]>
    {
        protected override byte[] CreateNew(int raw)
        {
            return new byte[raw];
        }

        protected override void DisposeOf(byte[] loaded) { }
    }

    public class TextureCache : Cache<IReadableTexture, LoadedTexture>
    {
        private static readonly FloatArrayCache floatsCache = new();
        private static readonly ByteArrayCache bytesCache = new();
        private const int componentCount = 4;

        public override LoadedTexture Load(IReadableTexture raw)
        {
            var loaded = base.Load(raw);

            if (raw.NeedsUpdate)
                UploadTexture(raw, loaded.Index);

            return loaded;
        }

        protected override LoadedTexture CreateNew(IReadableTexture raw)
        {
            var textureIndex = GL.GenTexture();
            UploadTexture(raw, textureIndex);
            return new LoadedTexture(raw.Width, raw.Height, textureIndex);
        }

        private void UploadTexture(IReadableTexture raw, int index)
        {
            GL.BindTexture(TextureTarget.Texture2D, index);

            SetTextureParameters(raw);

            var pixels = raw.ReadPixels();

            if (pixels.Length == raw.Width * raw.Height)
            {
                //TODO dit is een beetje lelijke code
                if (raw.HDR)
                    WriteHDRData(raw, componentCount, pixels);
                else
                    WriteLDRData(raw, componentCount, pixels);
            }
            else
            {
                if (raw is not RenderTexture)
                    Logger.Warn($"Texture data length is not equal to width * height! Expected: {raw.Width * raw.Height}, actual: {pixels.Length}");

                if (raw.HDR)
                    SetTextureData((float[])null, raw);
                else
                    SetTextureData((byte[])null, raw);
            }

            if (raw.GenerateMipmaps)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            raw.NeedsUpdate = false;

            if (raw.DisposeLocalCopyAfterUpload)
                raw.DisposeLocalCopy();
        }

        private void WriteLDRData(IReadableTexture raw, int componentCount, ReadOnlySpan<Color> pixels)
        {
            var data = bytesCache.Load(raw.Width * raw.Height * componentCount);
            int i = 0;
            foreach (var pixel in pixels)
            {
                data[i]     = (byte)(Utilities.Clamp(pixel.R) * 255);
                data[i + 1] = (byte)(Utilities.Clamp(pixel.G) * 255);
                data[i + 2] = (byte)(Utilities.Clamp(pixel.B) * 255);
                data[i + 3] = (byte)(Utilities.Clamp(pixel.A) * 255);

                i += componentCount;
            }
            SetTextureData(data, raw);
        }

        private void WriteHDRData(IReadableTexture raw, int componentCount, ReadOnlySpan<Color> pixels)
        {
            var data = floatsCache.Load(raw.Width * raw.Height * componentCount);
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

        private static void SetTextureParameters(IReadableTexture raw)
        {
            var wrap = (int)TypeConverter.Convert(raw.WrapMode);
            var maxFilter = (int)TypeConverter.Convert(raw.FilterMode);
            var mipmapFilter = (int)TextureMinFilter.LinearMipmapLinear;
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, raw.GenerateMipmaps ? mipmapFilter : maxFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, maxFilter); ;
        }

        private static void SetTextureData(byte[] data, IReadableTexture raw)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, raw.Width, raw.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        private static void SetTextureData(float[] data, IReadableTexture raw)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, raw.Width, raw.Height, 0, PixelFormat.Rgba, PixelType.Float, data);
        }

        protected override void DisposeOf(LoadedTexture loaded)
        {
            GL.DeleteTexture(loaded.Index);
            Logger.Log($"Deleted texture {loaded.Index}");
        }
    }
}
