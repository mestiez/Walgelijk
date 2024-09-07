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

    public readonly int Capacity;

    private readonly TransformComponent[] buffer;

    public TransformSystem(int capacity = 4096)
    {
        Capacity = capacity;
        buffer = new TransformComponent[capacity];
    }

    public override void Update()
    {
        var all = Scene.GetAllComponentsOfType(buffer);

        if (Parenting)
        {
            // reset
            foreach (var item in all)
                item.InternalChildren.Clear();

            // set children
            foreach (var item in all)
            {
                if (!item.Parent.HasValue)
                    continue;

                if (item.Parent.HasValue && item.Parent.Value.IsValid(Scene))
                    item.Parent.Value.Get(Scene).InternalChildren.Add(new ComponentRef<TransformComponent>(item.Entity));
            }

            // calculate transforms
            foreach (var item in all)
            {
                if (item.Parent.HasValue) // recursve from roots
                    continue;

                CalculateMatrix(item, Matrix3x2.Identity);
            }
        }
        else
            foreach (var item in all)
            {
                item.InternalChildren.Clear();
                CalculateMatrix(item, Matrix3x2.Identity, false);
            }
    }

    private void CalculateMatrix(TransformComponent transform, in Matrix3x2 model, bool recurse = true)
    {
        if (transform.InterpolationFlags != InterpolationFlags.None)
        {
            var f = transform.InterpolationFlags;
            var t = Time.Interpolation;

            var pos = (f & InterpolationFlags.Position) == InterpolationFlags.Position ?
                Utilities.Lerp(transform.PreviousPosition, transform.Position, t) :
                transform.Position;

            var rotation = (f & InterpolationFlags.Rotation) == InterpolationFlags.Rotation ?
                Utilities.LerpAngle(transform.PreviousRotation, transform.Rotation, t) :
                transform.Rotation;

            var scale = (f & InterpolationFlags.Scale) == InterpolationFlags.Scale ?
                Utilities.Lerp(transform.PreviousScale, transform.Scale, t) :
                transform.Scale;

            var localPivot = (f & InterpolationFlags.LocalPivot) == InterpolationFlags.LocalPivot ?
                Utilities.Lerp(transform.PreviousLocalPivot, transform.LocalPivot, t) :
                transform.LocalPivot;

            var localRotationPivot = (f & InterpolationFlags.LocalRotationPivot) == InterpolationFlags.LocalRotationPivot ?
                Utilities.Lerp(transform.PreviousLocalRotationPivot, transform.LocalRotationPivot, t) :
                transform.LocalRotationPivot;

            transform.RecalculateModelMatrix(model, pos, rotation, scale, localPivot, localRotationPivot);
        }
        else
            transform.RecalculateModelMatrix(model);

        if (recurse)
            foreach (var child in transform.InternalChildren)
                if (child.TryGet(Scene, out var childTransform))
                    CalculateMatrix(childTransform, transform.LocalToWorldMatrix);
    }
}
