namespace Walgelijk.OpenTK
{
    internal struct RenderTextureHandles
    {
        public readonly int FramebufferID;
        public readonly int TextureID;

        public RenderTextureHandles(int framebufferID, int textureID)
        {
            FramebufferID = framebufferID;
            TextureID = textureID;
        }
    }
}
