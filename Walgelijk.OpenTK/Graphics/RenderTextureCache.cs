using OpenTK.Graphics.OpenGL;

namespace Walgelijk.OpenTK
{
    internal class RenderTextureCache : Cache<RenderTexture, RenderTextureHandles>
    {
        protected override RenderTextureHandles CreateNew(RenderTexture raw)
        {
            int framebufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferID);

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
                return default;
            }

            return new RenderTextureHandles(framebufferID, loadedTexture.Index, raw);
        }

        protected override void DisposeOf(RenderTextureHandles loaded)
        {
            if (loaded.Raw != null)
            {
                GL.DeleteFramebuffer(loaded.FramebufferID);
                GPUObjects.TextureCache.Unload(loaded.Raw);
                GPUObjects.RenderTargetDictionary.Delete(loaded.Raw);
            }
        }
    }
}
