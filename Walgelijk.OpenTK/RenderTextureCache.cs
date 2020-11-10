using OpenTK.Graphics.OpenGL;

namespace Walgelijk.OpenTK
{
    internal class RenderTextureCache : Cache<RenderTexture, RenderTextureHandles>
    {
        protected override RenderTextureHandles CreateNew(RenderTexture raw)
        {
            int framebufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);

            var internalPixelFormat = raw.HDR ? PixelInternalFormat.Rgba16f : PixelInternalFormat.Rgba;
            //TODO HDR textures
            var loadedTexture = GPUObjects.TextureCache.Load(raw);

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer, 
                FramebufferAttachment.ColorAttachment0, 
                TextureTarget.Texture2D, 
                loadedTexture.Index, 
                0);

            var result = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (result != FramebufferErrorCode.FramebufferComplete)
            {
                Logger.Error($"Could not create RenderTexture: {result}");
                return new RenderTextureHandles(0, 0);
            }

            return new RenderTextureHandles(framebufferID, loadedTexture.Index);
        }

        protected override void DisposeOf(RenderTextureHandles loaded)
        {
            GL.DeleteFramebuffer(loaded.FramebufferID);
            GL.DeleteTexture(loaded.TextureID);

            //Unload rendertarget van rendertargetdictionary en texture van texturecache
        }
    }
}
