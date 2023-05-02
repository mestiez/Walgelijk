using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// The system responsible for processing transforms
/// </summary>
public class TransformSystem : System
{
    /// <summary>
    /// Enable transform parenting?
    /// </summary>
    public bool Parenting = true;

    public readonly int Capacity = 4096;

    private readonly TransformComponent[] buffer;

    public TransformSystem(int capacity = 4096)
    {
        Capacity = capacity;
        buffer = new TransformComponent[capacity];
    }

    public override void Update()
    {
        var all = Scene.GetAllComponentsOfType(buffer);

        // reset
        foreach (var item in all)
            item.InternalChildren.Clear();

        // set children
        foreach (var item in all)
        {
            if (!item.Parent.HasValue)
                continue;

            var parent = Scene.GetComponentFrom<TransformComponent>(item.Parent.Value);
            parent.InternalChildren.Add(item.Entity);
        }

        // calculate transforms
        foreach (var item in all)
        {
            if (item.Parent.HasValue) // recursve from roots
                continue;

            CalculateMatrix(item, Matrix3x2.Identity);
        }
    }

    private void CalculateMatrix(TransformComponent transform, in Matrix3x2 model)
    {
        if (transform.InterpolationFlags != InterpolationFlags.None)
        {
            var f = transform.InterpolationFlags;
            var t = Time.Interpolation;

            var pos =
                f.HasFlag(InterpolationFlags.Position) ?
                Utilities.Lerp(transform.PreviousPosition, transform.Position, t) :
                transform.Position;

            var rotation =
                f.HasFlag(InterpolationFlags.Rotation) ?
                Utilities.LerpAngle(transform.PreviousRotation, transform.Rotation, t) :
                transform.Rotation;

            var scale =
                f.HasFlag(InterpolationFlags.Scale) ?
                Utilities.Lerp(transform.PreviousScale, transform.Scale, t) :
                transform.Scale;

            var localPivot =
                f.HasFlag(InterpolationFlags.LocalPivot) ?
                Utilities.Lerp(transform.PreviousLocalPivot, transform.LocalPivot, t) :
                transform.LocalPivot;

            var localRotationPivot =
                f.HasFlag(InterpolationFlags.LocalRotationPivot) ?
                Utilities.Lerp(transform.PreviousLocalRotationPivot, transform.LocalRotationPivot, t) :
                transform.LocalRotationPivot;

            transform.RecalculateModelMatrix(model, pos, rotation, scale, localPivot, localRotationPivot);
        }
        else
            transform.RecalculateModelMatrix(model);

        foreach (var child in transform.Children)
            CalculateMatrix(Scene.GetComponentFrom<TransformComponent>(child), transform.LocalToWorldMatrix);
    }
}
