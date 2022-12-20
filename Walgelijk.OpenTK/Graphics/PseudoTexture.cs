using System;

namespace Walgelijk.OpenTK;

public class PseudoTexture : IReadableTexture
{
    public readonly int TextureHandle;
    private readonly int width;
    private readonly int height;

    public PseudoTexture(int textureHandle, int width, int height)
    {
        TextureHandle = textureHandle;
        this.width = width;
        this.height = height;
    }

    public int Width => width;
    public int Height => height;
    public WrapMode WrapMode => throw new NotImplementedException();
    public FilterMode FilterMode => throw new NotImplementedException();
    public bool HDR => throw new NotImplementedException();
    public bool GenerateMipmaps => throw new NotImplementedException();
    public bool NeedsUpdate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool DisposeLocalCopyAfterUpload => throw new NotImplementedException();
    public void Dispose() => Game.Main.Window.Graphics.Delete(this);
    public void DisposeLocalCopy() => throw new NotImplementedException();
    public ReadOnlyMemory<Color>? GetData() => throw new NotImplementedException();
    public Color GetPixel(int x, int y) => throw new NotImplementedException();
}
