﻿using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Walgelijk.OpenTK
{
    public class OpenTKGraphics : IGraphics
    {
        private RenderTarget currentTarget;
        private DrawBounds drawBounds;
        private bool drawBoundEnabledCache;
        private Material currentMaterial;

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
                }

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

        public void Clear(Color color)
        {
            GL.ClearColor(color.R, color.G, color.B, color.A);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void Draw(VertexBuffer vertexBuffer, Material material = null)
        {
            if (currentTarget == null)
                return;

            PrepareVertexBuffer(vertexBuffer, material);
            GL.DrawElements(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.AmountOfIndicesToRender ?? vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void DrawInstanced(VertexBuffer vertexBuffer, int instanceCount, Material material = null)
        {
            if (currentTarget == null)
                return;

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
            if (material != null)
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
            if (currentMaterial == material) return;
            currentMaterial = material;
            var loadedShader = GPUObjects.MaterialCache.Load(material);
            int prog = loadedShader.ProgramHandle;

            GPUObjects.MaterialTextureCache.ActivateTexturesFor(loadedShader);
            GLUtilities.SetBlendMode(material.BlendMode);
            GL.UseProgram(prog);
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

            GL.BlitNamedFramebuffer(
                sourceLoaded.FramebufferID,
                destinationLoaded.FramebufferID,
                0, 0, source.Width, source.Height,
                0, 0, destination.Width, destination.Height,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Linear
                );
        }

        public bool TryGetId(RenderTexture rt, out int frameBufferId, out int textureId)
        {
            textureId = frameBufferId = -1;
            if (GPUObjects.RenderTextureCache.Has(rt))
            {
                var l = GPUObjects.RenderTextureCache.Load(rt);
                frameBufferId = l.FramebufferID;
                textureId = l.TextureID;
                return true;
            }
            return false;
        }

        public bool TryGetId(IReadableTexture texture, out int textureId)
        {
            textureId = -1;
            if (GPUObjects.TextureCache.Has(texture))
            {
                textureId = GPUObjects.TextureCache.Load(texture).Index;
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
            if (TryGetId(texture, out var id))
            {
                using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(texture.Width, texture.Height);
                var frame = image.Frames.RootFrame;
                byte[] data = new byte[texture.Width * texture.Height * 4 * (texture.HDR ? 4 : 1)];


                if (texture is Texture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, id);
                    GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, texture.HDR ? PixelType.Float : PixelType.UnsignedByte, data);
                }
                else if (texture is RenderTexture)
                {
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, id);
                    GL.ReadPixels(0, 0, texture.Width, texture.Height, PixelFormat.Rgba, texture.HDR ? PixelType.Float : PixelType.UnsignedByte, data);
                }
                else throw new InvalidOperationException("Can only save Texture and RenderTexture");

                int i = 0;
                for (int yy = 0; yy < frame.Height; yy++)
                {
                    int y = (frame.Height - 1 - yy);
                    Span<SixLabors.ImageSharp.PixelFormats.Rgba32> pixelRowSpan = frame.PixelBuffer.DangerousGetRowSpan(y);
                    for (int x = 0; x < image.Width; x++)
                    {
                        pixelRowSpan[x] = new SixLabors.ImageSharp.PixelFormats.Rgba32(
                            data[i + 0],
                            data[i + 1],
                            data[i + 2],
                            data[i + 3]);
                        i += 4;
                    }
                }

                image.Save(output, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
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
}
