using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Render task that renders a vertex buffer with a material
/// </summary>
public class ShapeRenderTask : IRenderTask
{
    /// <summary>
    /// Create a shape render task
    /// </summary>
    public ShapeRenderTask(VertexBuffer vertexBuffer, Matrix3x2 modelMatrix = default, Material? material = null)
    {
        VertexBuffer = vertexBuffer;
        Material = material ?? Material.DefaultTextured;
        ModelMatrix = modelMatrix;
        ScreenSpace = false;
    }

    /// <summary>
    /// The matrix to transform the vertices with
    /// </summary>
    public Matrix3x2 ModelMatrix;
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
    /// <summary>
    /// The uniform to set to the sprite texture
    /// </summary>
    public string TintUniform = "tint";
    /// <summary>
    /// Sets the <see cref="TintUniform"/> uniform before rendering. If null, does nothing.
    /// </summary>
    public Color? Color { get; set; }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected virtual void Draw(IGraphics graphics)
    {
        graphics.CurrentTarget.ModelMatrix = new Matrix4x4(ModelMatrix);
        if (Color.HasValue)
            Material.SetUniform(TintUniform, Color.Value);
        graphics.Draw(VertexBuffer, Material);
    }
}
