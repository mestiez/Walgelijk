using OpenTK.Graphics.OpenGL;
using System;
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
            GL.DrawElements(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void DrawInstanced(VertexBuffer vertexBuffer, int instanceCount, Material material = null)
        {
            if (currentTarget == null)
                return;

            PrepareVertexBuffer(vertexBuffer, material);
            GL.DrawElementsInstanced(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, instanceCount);
        }

        public void SetUniform(Material material, string uniformName, object data)
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
    }
}
