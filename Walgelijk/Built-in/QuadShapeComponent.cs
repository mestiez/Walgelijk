using System.Numerics;

namespace Walgelijk;

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
        get => RenderTask.Material;
        set => RenderTask.Material = value;
    }

    /// <summary>
    /// Create a quad shape component
    /// </summary>
    public QuadShapeComponent(bool centered)
    {
        VertexBuffer = centered ? PrimitiveMeshes.CenteredQuad : PrimitiveMeshes.Quad;

        RenderTask = new ShapeRenderTask(VertexBuffer)
        {
            ModelMatrix = Matrix3x2.Identity,
            VertexBuffer = VertexBuffer
        };
    }
}
