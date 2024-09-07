using System.Numerics;

namespace Walgelijk;

/// <summary>
/// A target that can be rendered to
/// </summary>
public abstract class RenderTarget
{
    /// <summary>
    /// The view matrix
    /// </summary>
    public Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;
    /// <summary>
    /// The projection matrix
    /// </summary>
    public Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
    /// <summary>
    /// The model matrix
    /// </summary>
    public Matrix4x4 ModelMatrix { get; set; } = Matrix4x4.Identity;

    /// <summary>
    /// Size of the target.
    /// </summary>
    public abstract Vector2 Size { get; set; }

    /// <summary>
    /// Calculate the aspect ratio from the current render target size. Identical to Size.Y / Size.X
    /// </summary>
    public float AspectRatio => Size.Y / Size.X;

    /// <summary>
    /// An orthographic projection matrix where the top left is 0,0 and the bottom right is <see cref="Size"/>
    /// </summary>
    public Matrix4x4 OrthographicMatrix => Matrix4x4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, 0, 100);

    /// <summary>
    /// An orthographic projection matrix where the top left is 0,0 and the bottom right is 1, 1
    /// </summary>
    public Matrix4x4 UvMatrix = uvMatrix;

    /// <summary>
    /// This target's enabled features
    /// </summary>
    public RenderTargetFlags Flags { get; init; }

    /// <summary>
    /// Returns true if <see cref="Flags"/> contains the given flags
    /// </summary>
    public bool HasFlag(RenderTargetFlags f) => (Flags & f) == f;

    private static Matrix4x4 uvMatrix = Matrix4x4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 100);
}
