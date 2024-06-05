using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Render task that renders a quad with a given model matrix, material, and texture. The texture is set each time it is executed.
/// </summary>
public class SpriteRenderTask : IRenderTask
{
    /// <summary>
    /// The sprite texture to render
    /// </summary>
    public IReadableTexture Texture = Walgelijk.Texture.ErrorTexture;

    /// <summary>
    /// The uniform to set to the sprite texture
    /// </summary>
    public string MainTextureUniform = "mainTex"; 
    /// <summary>
    /// The uniform to set to the sprite texture
    /// </summary>
    public string TintUniform = "tint";
    /// <summary>
    /// The matrix to transform the vertices with
    /// </summary>
    public Matrix3x2 ModelMatrix;
    /// <summary>
    /// Material to draw with
    /// </summary>
    public Material Material;
    /// <summary>
    /// Should the task set the view matrix to <see cref="Matrix4x4.Identity"/> 
    /// </summary>
    public bool ScreenSpace;
    /// <summary>
    /// Sets the <see cref="TintUniform"/> uniform before rendering. If null, does nothing.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Create a sprite render task
    /// </summary>
    public SpriteRenderTask(IReadableTexture texture, Matrix3x2 modelMatrix = default, Material? material = null)
    {
        Texture = texture;
        Material = material ?? Material.DefaultTextured;
        ModelMatrix = modelMatrix;
        ScreenSpace = false;
    }

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
        Material.SetUniform(MainTextureUniform, Texture);
        if (Color.HasValue)
            Material.SetUniform(TintUniform, Color.Value);
        graphics.Draw(PrimitiveMeshes.CenteredQuad, Material);
    }
}
