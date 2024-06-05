using System.Numerics;

namespace Walgelijk;

/// <summary>
/// A special shape component that can only render quads with a texture
/// </summary>
public class SpriteComponent : Component
{
    /// <summary>
    /// Material that is drawn with
    /// </summary>
    public IReadableTexture Texture
    {
        get => RenderTask.Texture;
        set => RenderTask.Texture = value;
    }

    /// <summary>
    /// Material that is drawn with
    /// </summary>
    public Material Material
    {
        get => RenderTask.Material;
        set => RenderTask.Material = value;
    }

    /// <summary>
    /// The task that will be sent to the render queue
    /// </summary>
    public readonly SpriteRenderTask RenderTask;

    /// <summary>
    /// Determines if the sprite should be rendered in screenspace
    /// </summary>
    public bool ScreenSpace { get; set; }
    /// <summary>
    /// Order of the rendering task
    /// </summary>
    public RenderOrder RenderOrder { get; set; }
    /// <summary>
    /// Whether or not the sprite is rendered by the system
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Sets the "tint" uniform before rendering. If null, does nothing.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Apply horizontal flip
    /// </summary>
    public bool HorizontalFlip { get; set; } = false;
    /// <summary>
    /// Apply vertical flip
    /// </summary>
    public bool VerticalFlip { get; set; } = false;

    /// <summary>
    /// Create a SpriteComponent
    /// </summary>
    public SpriteComponent(IReadableTexture texture)
    {
        RenderTask = new SpriteRenderTask(texture)
        {
            ModelMatrix = Matrix3x2.Identity,
        };
    }
}
