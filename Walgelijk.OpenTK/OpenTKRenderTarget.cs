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

        private Matrix4x4 viewMatrix = Matrix4x4.Identity;
        private Matrix4x4 projectionMatrix = Matrix4x4.Identity;
        private Matrix4x4 modelMatrix = Matrix4x4.Identity;

        private bool viewMatrixChanged = false;
        private bool projectionMatrixChanged = false;

        private Material currentMaterial;

        private readonly VertexBufferCache vertexBufferCache = new VertexBufferCache();
        private OpenTKShaderManager ShaderManager => Window.shaderManager;

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

        public override Matrix4x4 ViewMatrix
        {
            get => viewMatrix;
            set
            {
                if (viewMatrix== value) 
                    viewMatrixChanged = true;
                viewMatrix = value;
            }
        }

        public override Matrix4x4 ProjectionMatrix
        {
            get => projectionMatrix;

            set
            {
                if (projectionMatrix != value)
                    projectionMatrixChanged = true;
                projectionMatrix = value;
            }
        }
        public override Matrix4x4 ModelMatrix { get => modelMatrix; set => modelMatrix = value; }

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

            GL.BindVertexArray(handles.VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handles.IBO);

            GL.DrawElements(TypeConverter.Convert(vertexBuffer.PrimitiveType), vertexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        private void SetTransformationMatrixUniforms(Material material)
        {
            if (viewMatrixChanged)
            {
                viewMatrixChanged = false;
                ShaderManager.SetUniform(material.Shader, ShaderConstants.ViewMatrixUniform, ViewMatrix);
            }

            if (projectionMatrixChanged)
            {
                projectionMatrixChanged = false;
                ShaderManager.SetUniform(material.Shader, ShaderConstants.ProjectionMatrixUniform, ProjectionMatrix);
            }

            ShaderManager.SetUniform(material.Shader, ShaderConstants.ModelMatrixUniform, ModelMatrix);
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

        private void SetMaterial(Material material)
        {
            if (currentMaterial == material) return;
            currentMaterial = material;
            var loadedShader = ShaderManager.ShaderCache.Load(material.Shader);
            int prog = loadedShader.ProgramHandle;
            GL.UseProgram(prog);
        }
    }
}
