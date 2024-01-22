using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Walgelijk.OpenTK;

public class OpenTKGraphics : IGraphics
{
    private RenderTarget currentTarget;
    private DrawBounds drawBounds;
    private bool drawBoundEnabledCache;
    private BlendMode currentBlendMode;
    private StencilState stencil;
    internal bool StencilUpdated = false;

    public DrawBounds DrawBounds
    {
        get => drawBounds;
        set
        {
            drawBounds = value;
            if (currentTarget != null)
                SetDrawbounds(value);
        }
    }

    public RenderTarget CurrentTarget
    {
        get => currentTarget;

        set
        {
            if (currentTarget != value)
                SetDrawbounds(drawBounds);

            currentTarget = value;

            if (currentTarget is RenderTexture rt)
            {
                var handles = GPUObjects.RenderTextureCache.Load(rt);
                GPUObjects.RenderTargetDictionary.Set(rt, handles.FramebufferID);

                if (rt.Flags.HasFlag(RenderTextureFlags.Depth))
                    GL.Enable(EnableCap.DepthTest);
                else
                    GL.Disable(EnableCap.DepthTest);
            }
            else
                GL.Disable(EnableCap.DepthTest);

            var id = GPUObjects.RenderTargetDictionary.Get(currentTarget);
            if (id == -1)
                Logger.Error("Attempt to set non-existent render target");
            else
            {
                var size = value.Size;
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
                GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            }
        }
    }

    public StencilState Stencil
    {
        get => stencil;
        set
        {
            StencilUpdated = true;
            stencil = value;
        }
    }

    public void Clear(Color color)
    {
        GL.ClearColor(color.R, color.G, color.B, color.A);
        GL.ClearDepth(1);
        GL.ClearStencil(1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
    }

    public void Draw(VertexBuffer vertexBuffer, Material material = null)
    {
        if (currentTarget == null)
            return;

        material ??= Material.DefaultTextured;

        UpdateStencilState();
        PrepareVertexBuffer(vertexBuffer, material);
        GL.DrawElements(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.AmountOfIndicesToRender ?? vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    private void UpdateStencilState()
    {
        if (StencilUpdated)
        {
            StencilUpdated = false;

            if (Stencil.Enabled)
            {
                GL.Enable(EnableCap.StencilTest);
                switch (Stencil.AccessMode)
                {
                    case StencilAccessMode.NoWrite:
                        GL.StencilMask(0x00);
                        break;
                    case StencilAccessMode.Write:
                        GL.StencilMask(0xFF);
                        break;
                }

                switch (stencil.TestMode)
                {
                    case StencilTestMode.Inside:
                        GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                        break;
                    case StencilTestMode.Outside:
                        GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                        break;
                }
            }
            else
                GL.Disable(EnableCap.StencilTest);
        }
    }

    public void DrawInstanced(VertexBuffer vertexBuffer, int instanceCount, Material material = null)
    {
        if (currentTarget == null)
            return;

        UpdateStencilState();
        PrepareVertexBuffer(vertexBuffer, material);
        GL.DrawElementsInstanced(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.AmountOfIndicesToRender ?? vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, instanceCount);
    }

    public void SetUniform<T>(Material material, string uniformName, T data)
    {
        ShaderManager.Instance.SetUniform(material, uniformName, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PrepareVertexBuffer(VertexBuffer vertexBuffer, Material material)
    {
        SetMaterial(material);
        SetTransformationMatrixUniforms(material);

        VertexBufferCacheHandles handles = GPUObjects.VertexBufferCache.Load(vertexBuffer);

        GL.BindVertexArray(handles.VAO);

        if (vertexBuffer.HasChanged)
            GPUObjects.VertexBufferCache.UpdateBuffer(vertexBuffer, handles);

        if (vertexBuffer.ExtraDataHasChanged)
            GPUObjects.VertexBufferCache.UpdateExtraData(vertexBuffer, handles);
    }

    private void SetTransformationMatrixUniforms(Material material)
    {
        ShaderManager.Instance.SetUniform(material, ShaderDefaults.ViewMatrixUniform, CurrentTarget.ViewMatrix);
        ShaderManager.Instance.SetUniform(material, ShaderDefaults.ProjectionMatrixUniform, CurrentTarget.ProjectionMatrix);
        ShaderManager.Instance.SetUniform(material, ShaderDefaults.ModelMatrixUniform, CurrentTarget.ModelMatrix);
    }

    private void SetMaterial(Material material)
    {
        if (currentBlendMode != material.BlendMode)
        {
            currentBlendMode = material.BlendMode;
            GLUtilities.SetBlendMode(material.BlendMode);
        }

        var loadedShader = GPUObjects.MaterialCache.Load(material);
        int prog = loadedShader.ProgramHandle;

        GPUObjects.MaterialTextureCache.ActivateTexturesFor(loadedShader);
        GL.UseProgram(prog);

        if (material.BackfaceCulling)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }
        else
        {
            GL.Disable(EnableCap.CullFace);
        }
    }

    private void SetDrawbounds(DrawBounds bounds)
    {
        if (!bounds.Enabled)
        {
            if (drawBoundEnabledCache)
                GL.Disable(EnableCap.ScissorTest);

            drawBoundEnabledCache = false;
            return;
        }

        bounds.Size.X = MathF.Max(0, bounds.Size.X);
        bounds.Size.Y = MathF.Max(0, bounds.Size.Y);

        int x = (int)MathF.Round(bounds.Position.X);
        int y = (int)MathF.Round(CurrentTarget.Size.Y - bounds.Position.Y - bounds.Size.Y);
        int w = (int)MathF.Round(bounds.Size.X);
        int h = (int)MathF.Round(bounds.Size.Y);

        GL.Scissor(x, y, w, h);
        if (!drawBoundEnabledCache)
            GL.Enable(EnableCap.ScissorTest);

        drawBoundEnabledCache = true;
    }

    public void Delete(object obj)
    {
        if (obj == null)
            return;

        switch (obj)
        {
            case RenderTexture rt:
                GPUObjects.RenderTextureCache.Unload(rt);
                break;
            case IReadableTexture texture:
                GPUObjects.TextureCache.Unload(texture);
                break;
            case VertexBuffer vb:
                GPUObjects.VertexBufferCache.Unload(vb);
                break;
            case Material mat:
                GPUObjects.MaterialCache.Unload(mat);
                break;
            case Shader shader:
                GPUObjects.ShaderCache.Unload(shader);
                break;
            default:
                Logger.Error("Attempt to delete unsupported object from GPU");
                break;
        }
    }

    public void Blit(RenderTexture source, RenderTexture destination)
    {
        var sourceLoaded = GPUObjects.RenderTextureCache.Load(source);
        var destinationLoaded = GPUObjects.RenderTextureCache.Load(destination);
        //var destinationLoaded = GPUObjects.RenderTargetDictionary.Get(destination);

        GL.BlitNamedFramebuffer(
            sourceLoaded.FramebufferID,
            destinationLoaded.FramebufferID,
            0, 0, source.Width, source.Height,
            0, 0, (int)destination.Size.X, (int)destination.Size.Y,
            ClearBufferMask.ColorBufferBit,
            BlitFramebufferFilter.Linear
            );
    }

    public bool TryGetId(RenderTexture rt, out int frameBufferId, out int[] textureId)
    {
        textureId = new int[] { -1 };
        frameBufferId = -1;
        if (GPUObjects.RenderTextureCache.Has(rt))
        {
            var l = GPUObjects.RenderTextureCache.Load(rt);
            frameBufferId = l.FramebufferID;
            textureId = l.TextureIds;
            return true;
        }
        return false;
    }

    public bool TryGetId(IReadableTexture texture, out int textureId)
    {
        textureId = -1;
        if (GPUObjects.TextureCache.Has(texture))
        {
            textureId = GPUObjects.TextureCache.Load(texture).Handle;
            return true;
        }
        return false;
    }

    public int TryGetId(VertexBuffer vb, out int vertexBufferId, out int indexBufferId, out int vertexArrayId, ref int[] vertexAttributeIds)
    {
        vertexArrayId = vertexBufferId = indexBufferId = -1;
        if (GPUObjects.VertexBufferCache.Has(vb))
        {
            var l = GPUObjects.VertexBufferCache.Load(vb);
            int extraVboLength = Math.Min(l.ExtraVBO.Length, vertexAttributeIds.Length);
            for (int i = 0; i < extraVboLength; i++)
                vertexAttributeIds[i] = l.ExtraVBO[i];
            indexBufferId = l.VAO;
            vertexArrayId = l.VAO;
            vertexBufferId = l.VBO;
            return extraVboLength;
        }
        return -1;
    }

    public void SaveTexture(FileStream output, IReadableTexture texture)
    {
        switch (texture)
        {
            case Texture tex:
                {
                    if (TryGetId(tex, out var id))
                    {
                        using var img = TextureToImage(texture.HDR, texture.Width, texture.Height, id);
                        img.SaveAsPng(output);
                    }
                }
                break;
            case RenderTexture rt:
                {
                    if (TryGetId(rt, out var frameBufferId, out var texInts))
                    {
                        var final = new Image<Rgba32>(rt.Width, rt.Height * (rt.DepthBuffer != null ? 2 : 1));

                        int s = rt.Width * rt.Height * 4;

                        if ((rt.Flags & RenderTextureFlags.HDR) != RenderTextureFlags.None)
                        {
                            var data = new float[s];
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferId);
                            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                            GL.ReadPixels(0, 0, texture.Width, texture.Height, PixelFormat.Rgba, PixelType.Float, data);
                            var a = BuildImage(texture.Width, texture.Height, data);
                            final.Mutate(i =>
                            {
                                i.DrawImage(a, 1);
                            });
                            a.Dispose();
                        }
                        else
                        {
                            var data = new byte[s];
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferId);
                            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                            GL.ReadPixels(0, 0, texture.Width, texture.Height, PixelFormat.Rgba, PixelType.Float, data);
                            var a = BuildImage(texture.Width, texture.Height, data);
                            final.Mutate(i =>
                            {
                                i.DrawImage(a, 1);
                            });
                            a.Dispose();
                        }

                        int y = rt.Height;

                        if (rt.DepthBuffer != null)
                        {
                            var data = new float[s / 4];
                            var data2 = new float[s];
                            GL.ReadPixels(0, 0, texture.Width, texture.Height, PixelFormat.DepthComponent, PixelType.Float, data);

                            for (int i = 0; i < data2.Length; i += 4)
                                data2[i] = data2[i + 1] = data2[i + 2] = data2[i + 3] = data[i / 4];

                            var a = BuildImage(texture.Width, texture.Height, data2);
                            final.Mutate(i =>
                            {
                                i.DrawImage(a, new Point(0, y), 1);
                            });
                            a.Dispose();
                        }

                        final.SaveAsPng(output);
                        final.Dispose();
                    }
                }
                break;
        }

        static Image BuildImage<T>(int width, int height, T[] data)
        {
            var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height);
            var frame = image.Frames.RootFrame;

            Func<int, Rgba32> toColor = data switch
            {
                byte[] b => i => new Rgba32(b[i], b[i + 1], b[i + 2], b[i + 3]),
                float[] f => i => new Rgba32(f[i], f[i + 1], f[i + 2], f[i + 3]),
                _ => throw new Exception("Attempt to save a texture with an invalid format: this error is so severe that you should stop programming forever."),
            };
            int i = 0;
            for (int yy = 0; yy < frame.Height; yy++)
            {
                int y = (frame.Height - 1 - yy);
                var pixelRowSpan = frame.PixelBuffer.DangerousGetRowSpan(y);
                for (int x = 0; x < image.Width; x++)
                {
                    pixelRowSpan[x] = toColor(i);
                    i += 4; // 4 compoments per pixel :)
                }
            }

            return image;
        }

        static Image TextureToImage(bool hdr, int w, int h, int id)
        {
            int s = w * h * 4;

            if (hdr)
            {
                var data = new float[s];
                GL.BindTexture(TextureTarget.Texture2D, id);
                GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.Float, data);
                return BuildImage(w, h, data);
            }
            else
            {
                var data = new byte[s];
                GL.BindTexture(TextureTarget.Texture2D, id);
                GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                return BuildImage(w, h, data);
            }
        }
    }

    public bool TryGetId(Material mat, out int id)
    {
        id = -1;
        if (GPUObjects.MaterialCache.Has(mat))
        {
            id = GPUObjects.MaterialCache.Load(mat).ProgramHandle;
            return true;
        }
        return false;
    }

    public void Upload(object obj)
    {
        switch (obj)
        {
            case RenderTexture rt:
                GPUObjects.RenderTextureCache.Load(rt);
                break;
            case IReadableTexture texture:
                GPUObjects.TextureCache.Load(texture);
                break;
            case VertexBuffer vb:
                GPUObjects.VertexBufferCache.Load(vb);
                break;
            case Material mat:
                GPUObjects.MaterialCache.Load(mat);
                break;
            default:
                Logger.Error("Attempt to upload unsupported object to GPU");
                break;
        }
    }
}
