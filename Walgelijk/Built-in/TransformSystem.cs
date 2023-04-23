using System;
using System.Buffers;
using System.Numerics;

namespace Walgelijk
{
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

        public TransformSystem(int capacity = 4096)
        {
            Capacity = capacity;
        }

        public override void Update()
        {
            var arr = ArrayPool<TransformComponent>.Shared.Rent(Capacity);
            var all = Scene.GetAllComponentsOfType(arr);

            for (int i = 0; i < all.Length; i++)
            {
                var transform = all[i];
                if (Parenting && !transform.Parent.HasValue)
                    CascadeMatrixCalculation(transform, all);
                else
                    CalculateMatrix(transform, Matrix3x2.Identity);
            }

            ArrayPool<TransformComponent>.Shared.Return(arr);
        }

        private bool CalculateMatrix(TransformComponent transform, in Matrix3x2 model)
        {
            bool shouldRecalculate = !transform.IsMatrixCached || transform.InterpolateBetweenFixedUpdates;

            if (shouldRecalculate)
            {
                if (transform.InterpolateBetweenFixedUpdates)
                {
                    var t = Time.Interpolation;
                    var pos = Utilities.Lerp(transform.PreviousPosition, transform.Position, t);
                    var rotation = Utilities.Lerp(transform.PreviousRotation, transform.Rotation, t);
                    var scale = Utilities.Lerp(transform.PreviousScale, transform.Scale, t);
                    var localPivot = Utilities.Lerp(transform.PreviousLocalPivot, transform.LocalPivot, t);
                    var localRotationPivot = Utilities.Lerp(transform.PreviousLocalRotationPivot, transform.LocalRotationPivot, t);
                    transform.RecalculateModelMatrix(model, pos, rotation, scale, localPivot, localRotationPivot);
                }
                else
                    transform.RecalculateModelMatrix(model);
                return true;
            }

            return false;
        }

        private void CascadeMatrixCalculation(TransformComponent transform, ReadOnlySpan<TransformComponent> all, TransformComponent? up = null)
        {
            bool recalculated = CalculateMatrix(transform, up?.LocalToWorldMatrix ?? Matrix3x2.Identity);

            // waarom heeft een transform geen lijst met kinderen? dat is sneller toch? 
            // helaas is het niet zo simpel. de lijst is dat een List<Entity> (waarschijnlijk) en die moet bijgehouden worden als een entity niet meer bestaat of als die entity geen Transform meer heeft.
            // dat is extra overhead. of dit het waard is is een ander verhaal. als je dat wilt meten, ga je gang, ik heb daar nu geen zin in.
            for (int i = 0; i < all.Length; i++)
            {
                var e = all[i];
                var myParent = e.Parent;

                if (myParent.HasValue && myParent.Value != e.Entity && myParent.Value == transform.Entity)
                {
                    e.IsMatrixCached &= !recalculated;
                    CascadeMatrixCalculation(e, all, transform);
                }
            }
        }
    }
}
