namespace Walgelijk.OpenTK
{
    internal struct RenderTextureHandles
    {
        public readonly int FramebufferID;
        public readonly int TextureID;
        public readonly RenderTexture Raw;

        public RenderTextureHandles(int framebufferID, int textureID, RenderTexture raw)
        {
            FramebufferID = framebufferID;
            TextureID = textureID;
            Raw = raw;
        }
    }
}
