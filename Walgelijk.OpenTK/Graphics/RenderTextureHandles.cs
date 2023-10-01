namespace Walgelijk.OpenTK;

internal struct RenderTextureHandles
{
    public readonly int FramebufferID;
    public readonly int[] TextureIds;

    public readonly RenderTexture RenderTexture;

    public RenderTextureHandles(int framebufferID, int[] textureID, RenderTexture raw)
    {
        FramebufferID = framebufferID;
        TextureIds = textureID;
        RenderTexture = raw;
    }
}
