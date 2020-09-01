using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that renders a vertex buffer with a material
    /// </summary>
    public class ShapeRenderTask : IRenderTask
    {
        /// <summary>
        /// Create a shape render task
        /// </summary>
        public ShapeRenderTask(VertexBuffer vertexBuffer, Matrix4x4 modelMatrix = default, Material material = null)
        {
            VertexBuffer = vertexBuffer;
            Material = material ?? Material.DefaultTextured;
            ModelMatrix = modelMatrix;
            ScreenSpace = false;
        }

        /// <summary>
        /// The matrix to transform the vertices with
        /// </summary>
        public Matrix4x4 ModelMatrix;
        /// <summary>
        /// Vertex buffer to draw
        /// </summary>
        public VertexBuffer VertexBuffer;
        /// <summary>
        /// Material to draw with
        /// </summary>
        public Material Material;
        /// <summary>
        /// Should the task set the view matrix to <see cref="Matrix4x4.Identity"/> 
        /// </summary>
        public bool ScreenSpace;

        public void Execute(RenderTarget target)
        {
            if (ScreenSpace)
                DrawScreenSpace(target);
            else
                Draw(target);
        }

        private void DrawScreenSpace(RenderTarget target)
        {
            var view = target.ViewMatrix;
            var proj = target.ProjectionMatrix;
            target.ProjectionMatrix = target.CalculatedWindowMatrix;
            target.ViewMatrix = Matrix4x4.Identity;

            Draw(target);

            target.ViewMatrix = view;
            target.ProjectionMatrix = proj;
        }

        private void Draw(RenderTarget target)
        {
            target.ModelMatrix = ModelMatrix;
            target.Draw(VertexBuffer, Material);
        }
    }
}
