using System;
using System.Numerics;

namespace Walgelijk;

public class CircleShapeComponent :  ShapeComponent, IDisposable
{
    public Color Color = Colors.White;
    public CircleShapeComponent(float size = 1, Color? color = null)
    {
        Color = color ?? Color.Red;

        VertexBuffer = PrimitiveMeshes.GenerateCircle(48, size);
        for (var index = 0; index < VertexBuffer.Vertices.Length; index++)
        {
            VertexBuffer.Vertices[index].Color = Color;
        }

        RenderTask = new ShapeRenderTask(VertexBuffer)
        {
            ModelMatrix = Matrix3x2.Identity,
        };
    }

    public void Dispose()
    {
        VertexBuffer.Dispose();
    }
}