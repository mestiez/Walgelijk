using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK;

internal class RenderTextureCache : Cache<RenderTexture, RenderTextureHandles>
{
    protected override RenderTextureHandles CreateNew(RenderTexture raw)
    {
        var framebufferId = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
        var ids = new List<int>();

        var textureTarget = TextureTarget.Texture2D;
        if (raw.HasFlag(RenderTargetFlags.Multisampling))
            textureTarget = TextureTarget.Texture2DMultisample;

        // generate color buffer
        {
            var id = GPUObjects.TextureCache.Load(raw).Handle;
            ids.Add(id);
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                textureTarget,
                id,
                0);
        }

        // generate depth buffer and stencil buffer
        if (raw.HasFlag(RenderTargetFlags.DepthStencil))
        {
            var id = GL.GenTexture(); // we will bypass the texture cache because it is completely unnecessary here
            ids.Add(id);
            GL.BindTexture(textureTarget, id);
            GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Depth24Stencil8, raw.Width, raw.Height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);

            var maxFilter = (int)TypeConverter.Convert(raw.FilterMode);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, maxFilter);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, maxFilter);

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthStencilAttachment,
                textureTarget,
                id,
                0);

            raw.DepthBuffer = new PseudoTexture(id, raw.Width, raw.Height);
            GL.BindTexture(textureTarget, 0);
        }

        var result = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (result != FramebufferErrorCode.FramebufferComplete)
        {
            Logger.Error($"Could not create RenderTexture: {result}");
            return new RenderTextureHandles(-1, null!, raw);
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
        GL.ClearColor(default(Color4));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        return new RenderTextureHandles(framebufferId, [.. ids], raw);
    }

    protected override void DisposeOf(RenderTextureHandles loaded)
    {
        if (loaded.RenderTexture == null)
            return;

        loaded.RenderTexture.DepthBuffer?.Dispose();
        loaded.RenderTexture.DepthBuffer = null;

        if (loaded.FramebufferID != -1)
            GL.DeleteFramebuffer(loaded.FramebufferID);
        GPUObjects.TextureCache.Unload(loaded.RenderTexture);
        GPUObjects.RenderTargetDictionary.Delete(loaded.RenderTexture);
        for (int i = 0; i < loaded.TextureIds.Length; i++)
        {
            if (loaded.TextureIds[i] != 0)
                GL.DeleteTexture(loaded.TextureIds[i]);
        }

        if (GL.GetInteger(GetPName.FramebufferBinding) == loaded.FramebufferID)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}
