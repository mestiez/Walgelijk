using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Primitive quad component. Vertices can't be altered.
    /// </summary>
    public class QuadShapeComponent : ShapeComponent
    {
        /// <summary>
        /// Material that is drawn with
        /// </summary>
        public Material Material
        {
            get => RenderTask.Material; set
            {
                var rt = RenderTask;
                rt.Material = value;
                RenderTask = rt;
            }
        }

        /// <summary>
        /// Create a quad shape component
        /// </summary>
        public QuadShapeComponent(bool centered)
        {
            VertexBuffer = centered ? PrimitiveMeshes.CenteredQuad : PrimitiveMeshes.Quad;

            RenderTask = new ShapeRenderTask(VertexBuffer)
            {
                ModelMatrix = Matrix4x4.Identity,
                VertexBuffer = VertexBuffer
            };
        }
    }   
}
