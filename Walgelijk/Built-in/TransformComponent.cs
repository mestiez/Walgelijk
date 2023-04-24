using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Basic component that holds transformation data
/// </summary>
public class TransformComponent : Component
{
    private Entity? parent;

    private Vector2 position;
    private float rotation;
    private Vector2 scale = Vector2.One;
    private Vector2 pivot = Vector2.Zero;
    private Vector2 rotationPivot = Vector2.Zero;

    public Vector2 PreviousPosition { get; private set; }
    public float PreviousRotation { get; private set; }
    public Vector2 PreviousScale { get; private set; } = Vector2.One;
    public Vector2 PreviousLocalPivot { get; private set; }
    public Vector2 PreviousLocalRotationPivot { get; private set; }

    public InterpolationFlags InterpolationFlags = InterpolationFlags.None;

    /// <summary>
    /// Parent entity with a transform
    /// </summary>
    public Entity? Parent
    {
        get => parent;
        set
        {
            if (parent == value)
                return;
            parent = value;
            IsMatrixCached = false;
        }
    }

    /// <summary>
    /// Position of the transform in parent space (global if no parent)
    /// </summary>
    public Vector2 Position
    {
        get => position;

        set
        {
            IsMatrixCached = false;
            PreviousPosition = position;
            position = value;
        }
    }

    /// <summary>
    /// Rotation in degrees of the transform in world space
    /// </summary>
    public float Rotation
    {
        get => rotation;

        set
        {
            IsMatrixCached = false;
            PreviousRotation = rotation;
            rotation = value;
        }
    }

    /// <summary>
    /// Scale multiplier of the transform
    /// </summary>
    public Vector2 Scale
    {
        get => scale;

        set
        {
            IsMatrixCached = false;
            PreviousScale = scale;
            scale = value;
        }
    }

    /// <summary>
    /// The local pivot point that is the center of all transformations.
    /// </summary>
    public Vector2 LocalPivot
    {
        get => pivot;

        set
        {
            IsMatrixCached = false;
            PreviousLocalPivot = pivot;
            pivot = new(value.X, value.Y);
        }
    }

    /// <summary>
    /// The local pivot point that is the center of rotation.
    /// </summary>
    public Vector2 LocalRotationPivot
    {
        get => rotationPivot;

        set
        {
            PreviousLocalRotationPivot = rotationPivot;
            IsMatrixCached = false;//&= value == rotationPivot;
            rotationPivot = new(value.X, value.Y);
        }
    }

    /// <summary>
    /// Returns if the model matrix is aligned to the transformation
    /// </summary>
    public bool IsMatrixCached
    {
        get => isMatrixCached && (InterpolationFlags == InterpolationFlags.None);
        internal set => isMatrixCached = value;
    }

    /// <summary>
    /// The generated model matrix
    /// </summary>
    public Matrix3x2 LocalToWorldMatrix;

    /// <summary>
    /// The inverse model matrix
    /// </summary>
    public Matrix3x2 WorldToLocalMatrix;

    /// <summary>
    /// All transform matrices separated by step. Useful for transforming specific vectors to/from local space
    /// </summary>
    public SeparateTransformMatrices SeparateMatrices;

    private bool isMatrixCached;

    /// <summary>
    /// Recalculate the model matrix considering a containing matrix. This is usually <see cref="Matrix3x2.Identity"/>
    /// </summary>
    public void RecalculateModelMatrix(Matrix3x2 containingMatrix, Vector2 position, float rotation, Vector2 scale, Vector2 pivot, Vector2 rotationPivot)
    {
        var matrix = Matrix3x2.Identity;

        if (pivot != Vector2.Zero)
            matrix = Matrix3x2.CreateTranslation(-pivot.X, -pivot.Y);
        SeparateMatrices.AfterPivot = matrix;

        if (scale != Vector2.One)
            matrix *= Matrix3x2.CreateScale(scale.X, scale.Y);
        SeparateMatrices.AfterScale = matrix;

        if (rotation % 360 != 0)
            matrix *= Matrix3x2.CreateRotation(rotation * Utilities.DegToRad, new Vector2(rotationPivot.X * scale.X, rotationPivot.Y * scale.Y));
        SeparateMatrices.AfterRotation = matrix;

        if (position != Vector2.Zero)
            matrix *= Matrix3x2.CreateTranslation(position.X, position.Y);
        SeparateMatrices.AfterTranslation = matrix;

        if (!containingMatrix.IsIdentity)
            matrix *= containingMatrix;

        SeparateMatrices.FinalModel = matrix;

        LocalToWorldMatrix = matrix;

        if (Matrix3x2.Invert(LocalToWorldMatrix, out var result))
            WorldToLocalMatrix = result;
        else
            WorldToLocalMatrix = Matrix3x2.Identity;

        IsMatrixCached = true;
    }

    /// <summary>
    /// Recalculate the model matrix considering a containing matrix. This is usually <see cref="Matrix3x2.Identity"/>
    /// </summary>
    public void RecalculateModelMatrix(Matrix3x2 containingMatrix)
    {
        RecalculateModelMatrix(containingMatrix, position, rotation, scale, pivot, rotationPivot);
    }
}
