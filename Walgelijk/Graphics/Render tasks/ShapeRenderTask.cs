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

        public void Execute(IGraphics graphics)
        {
            if (ScreenSpace)
                DrawScreenSpace(graphics);
            else
                Draw(graphics);
        }

        private void DrawScreenSpace(IGraphics graphics)
        {
            var target = graphics.CurrentTarget;

            var view = target.ViewMatrix;
            var proj = target.ProjectionMatrix;
            target.ProjectionMatrix = target.OrthographicMatrix;
            target.ViewMatrix = Matrix4x4.Identity;

            Draw(graphics);

            target.ViewMatrix = view;
            target.ProjectionMatrix = proj;
        }

        protected virtual void Draw(IGraphics graphics)
        {
            graphics.CurrentTarget.ModelMatrix = ModelMatrix;
            graphics.Draw(VertexBuffer, Material);
        }
    }
}
