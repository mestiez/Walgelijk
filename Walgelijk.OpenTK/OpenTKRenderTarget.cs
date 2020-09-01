using OpenTK.Graphics.OpenGL;
using System.Collections;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{
    public class OpenTKRenderTarget : Walgelijk.RenderTarget
    {
        internal OpenTKWindow Window { get; set; }

        private Vector2 size;
        private Color clearColour;

        private Material currentMaterial;

        private readonly VertexBufferCache vertexBufferCache = new VertexBufferCache();
        private OpenTKShaderManager ShaderManager => Window.shaderManager;

        public override Matrix4x4 ViewMatrix { get; set; }
        public override Matrix4x4 ProjectionMatrix { get; set; }
        public override Matrix4x4 ModelMatrix { get; set; }

        public override Vector2 Size
        {
            get => size;

            set
            {
                size = value;
                GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            }
        }

        public override Color ClearColour
        {
            get => clearColour;

            set
            {
                clearColour = value;
                GL.ClearColor(ClearColour.R, ClearColour.G, ClearColour.B, ClearColour.A);
            }
        }

        internal void Initialise()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public override void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public override void Draw(VertexBuffer vertexBuffer, Material material)
        {

            SetMaterial(material);
            if (material != null)
            {
                SetTransformationMatrixUniforms(material);
            }

            VertexBufferCacheHandles handles = vertexBufferCache.Load(vertexBuffer);
            if (vertexBuffer.HasChanged)
                vertexBufferCache.UpdateBuffer(vertexBuffer, handles);

            GL.BindVertexArray(handles.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handles.IBO);

            GL.DrawElements(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public override void Draw(Vertex[] vertices, Primitive primitive, Material material = null)
        {
            SetMaterial(material);

            GL.Begin(TypeConverter.Convert(primitive));

            foreach (var item in vertices)
            {
                GL.Color4(item.Color.R, item.Color.G, item.Color.B, item.Color.A);
                GL.TexCoord2(item.TexCoords.X, item.TexCoords.Y);
                GL.Vertex3(item.Position.X, item.Position.Y, item.Position.Z);
            }

            GL.End();
        }

        private void SetTransformationMatrixUniforms(Material material)
        {
            ShaderManager.SetUniform(material, ShaderDefaults.ViewMatrixUniform, ViewMatrix);
            ShaderManager.SetUniform(material, ShaderDefaults.ProjectionMatrixUniform, ProjectionMatrix);
            ShaderManager.SetUniform(material, ShaderDefaults.ModelMatrixUniform, ModelMatrix);
        }

        private void SetMaterial(Material material)
        {
            if (currentMaterial == material) return;
            currentMaterial = material;
            var loadedShader = ShaderManager.MaterialCache.Load(material);
            int prog = loadedShader.ProgramHandle;

            ShaderManager.TextureCache.ActivateTexturesFor(loadedShader);
            GL.UseProgram(prog);
        }
    }
}
