using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that renders an instanced vertex buffer with a material
    /// </summary>
    public class InstancedShapeRenderTask : ShapeRenderTask
    {
        /// <summary>
        /// Create a shape render task
        /// </summary>
        public InstancedShapeRenderTask(VertexBuffer vertexBuffer, Matrix4x4 modelMatrix = default, Material material = null) : base(vertexBuffer, modelMatrix, material)
        {
        }

        /// <summary>
        /// Amount of instances 
        /// </summary>
        public int InstanceCount;

        protected override void Draw(IGraphics graphics)
        {
            graphics.CurrentTarget.ModelMatrix = ModelMatrix;
            graphics.DrawInstanced(VertexBuffer, InstanceCount, Material);
        }
    }
}
