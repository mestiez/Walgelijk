using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// A render target of an arbitrary size that can be used as a texture
/// </summary>
public class RenderTexture : RenderTarget, IReadableTexture, IDisposable
{
    private Vector2 size;
    private WrapMode wrapMode;
    private FilterMode filterMode;
    private bool disposed = false;

    /// <summary>
    /// Construct a <see cref="RenderTexture"/>
    /// </summary>
    [Obsolete("Use the constructor that takes RenderTextureFlags")]
    public RenderTexture(int width, int height, WrapMode wrapMode = WrapMode.Clamp, FilterMode filterMode = FilterMode.Linear, bool generateMipmaps = false, bool hdr = false)
    {
        this.size = new Vector2(width, height);
        this.wrapMode = wrapMode;
        this.filterMode = filterMode;
        if (generateMipmaps)
            Flags |= RenderTargetFlags.Mipmaps;
        if (hdr)
            Flags |= RenderTargetFlags.HDR;
    }

    public RenderTexture(int width, int height, WrapMode wrapMode = WrapMode.Clamp, FilterMode filterMode = FilterMode.Linear, RenderTargetFlags flags = RenderTargetFlags.None)
    {
        this.size = new Vector2(width, height);
        this.wrapMode = wrapMode;
        this.filterMode = filterMode;
        Flags = flags;
    }

    public override Vector2 Size
    {
        get => size;
        set
        {
            if (value != size)
                NeedsUpdate = true;

            size = value;
        }
    }

    public int Width => (int)Size.X;
    public int Height => (int)Size.Y;

    public WrapMode WrapMode
    {
        get => wrapMode;
        set
        {
            wrapMode = value;
            if (value != wrapMode)
                NeedsUpdate = true;
        }
    }

    public FilterMode FilterMode
    {
        get => filterMode;
        set
        {
            filterMode = value;
            if (value != filterMode)
                NeedsUpdate = true;
        }
    }

    public bool NeedsUpdate { get; set; }

    [Obsolete("Use Flags")]
    public bool GenerateMipmaps => Flags.HasFlag(RenderTargetFlags.Mipmaps);
    [Obsolete("Use Flags")]
    public bool HDR => Flags.HasFlag(RenderTargetFlags.HDR);

    /// <summary>
    /// Access to the depth (and stencil) buffer. This is null if <see cref="Flags"/> does not contain <see cref="RenderTargetFlags.DepthStencil"/>.
    /// <b>Note that this buffer may be being written to as you're reading from it, which will cause unsightly graphical artifacts. To be safe, blit the texture to somewhere else to read from!</b>
    /// </summary>
    public IReadableTexture? DepthBuffer;

    /// <summary>
    /// There is no local copy so this will do nothing.
    /// </summary>
    public bool DisposeLocalCopyAfterUpload => false;

    /// <summary>
    /// Returns <see cref="IGraphics.SampleTexture(IReadableTexture, int, int)"/>
    /// </summary>
    public Color GetPixel(int x, int y) => Game.Main.Window.Graphics.SampleTexture(this, x, y);

    /// <summary>
    /// You can't get pixels from a RenderTexture. This will return an empty span.
    /// </summary>
    public ReadOnlySpan<Color> ReadPixels() => [];

    /// <summary>
    /// Force an update
    /// </summary>
    public void ForceUpdate()
    {
        NeedsUpdate = true;
    }

    /// <summary>
    /// Delete this object from the GPU. This also makes it unusable on the CPU.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            Game.Main?.Window?.Graphics?.Delete(this);
            GC.SuppressFinalize(this);
        }
        disposed = true;
    }

    /// <summary>
    /// There is no local copy so this will do nothing.
    /// </summary>
    public void DisposeLocalCopy() { }

    /// <summary>
    /// You can't get pixels from a RenderTexture. This will return a region of zero length.
    /// </summary>
    public ReadOnlyMemory<Color>? GetData() => ReadOnlyMemory<Color>.Empty;
}
