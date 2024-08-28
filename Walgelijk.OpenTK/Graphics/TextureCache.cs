using OpenTK.Graphics.OpenGL4;
using System;
using System.Buffers;

namespace Walgelijk.OpenTK;

public class TextureCache : Cache<IReadableTexture, LoadedTexture>
{
    private const int componentCount = 4;

    public override LoadedTexture Load(IReadableTexture raw)
    {
        if (raw is PseudoTexture ps)
            return new LoadedTexture(ps.Width, ps.Height, ps.TextureHandle);

        var loaded = base.Load(raw);

        if (raw.NeedsUpdate)
            UploadTexture(raw, loaded.Handle);

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
        int l = raw.Width * raw.Height * componentCount;
        var data = ArrayPool<byte>.Shared.Rent(l);
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
        ArrayPool<byte>.Shared.Return(data);
    }

    private void WriteHDRData(IReadableTexture raw, int componentCount, ReadOnlySpan<Color> pixels)
    {
        int l = raw.Width * raw.Height * componentCount;
        var data = ArrayPool<float>.Shared.Rent(l);
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
        ArrayPool<float>.Shared.Return(data);
    }

    private static void SetTextureParameters(IReadableTexture raw)
    {
        var wrap = (int)TypeConverter.Convert(raw.WrapMode);
        var maxFilter = (int)TypeConverter.Convert(raw.FilterMode);
        const int mipmapFilter = (int)TextureMinFilter.LinearMipmapLinear;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrap);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrap);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, raw.GenerateMipmaps ? mipmapFilter : maxFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, maxFilter);
    }

    private static void SetTextureData(byte[] data, IReadableTexture raw)
    {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, raw.Width, raw.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    private static void SetTextureData(float[] data, IReadableTexture raw)
    {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, raw.Width, raw.Height, 0, PixelFormat.Rgba, PixelType.Float, data);
    }

    protected override void DisposeOf(LoadedTexture loaded)
    {
        GL.DeleteTexture(loaded.Handle);
        Logger.Log($"Deleted texture {loaded.Handle}");
    }
}
