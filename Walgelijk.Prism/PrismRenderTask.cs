using System.Numerics;

namespace Walgelijk.Prism;

public class PrismRenderTask : IRenderTask
{
    public PrismRenderTask(VertexBuffer vertexBuffer, Matrix4x4 modelMatrix = default, Material? material = null)
    {
        VertexBuffer = vertexBuffer;
        Material = material ?? new Material(Material.DefaultTextured) { DepthTested = true };
        ModelMatrix = modelMatrix;
        ScreenSpace = false;
    }

    public Matrix4x4 ModelMatrix;
    public VertexBuffer VertexBuffer;
    public Material Material;
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
