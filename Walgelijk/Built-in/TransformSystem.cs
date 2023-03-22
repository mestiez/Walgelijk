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
        /// Should the system use multithreading to calculate transform matrices?
        /// </summary>
        //public bool Multithreading = false;

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
                    CalculateMatrix(transform);
            }

            ArrayPool<TransformComponent>.Shared.Return(arr);
        }

        private void CalculateMatrix(TransformComponent transform)
        {
            bool shouldRecalculate = !transform.IsMatrixCached;

            if (shouldRecalculate)
                transform.RecalculateModelMatrix(Matrix3x2.Identity);
        }

        private void CascadeMatrixCalculation(TransformComponent transform, ReadOnlySpan<TransformComponent> all, TransformComponent? up = null)
        {
            bool shouldRecalculate = !transform.IsMatrixCached;

            if (shouldRecalculate)
                transform.RecalculateModelMatrix(up?.LocalToWorldMatrix ?? Matrix3x2.Identity);

            // waarom heeft een transform geen lijst met kinderen? dat is sneller toch? 
            // helaas is het niet zo simpel. de lijst is dat een List<Entity> (waarschijnlijk) en die moet bijgehouden worden als een entity niet meer bestaat of als die entity geen Transform meer heeft.
            // dat is extra overhead. of dit het waard is is een ander verhaal. als je dat wilt meten, ga je gang, ik heb daar nu geen zin in.
            for (int i = 0; i < all.Length; i++)
            {
                var e = all[i];
                var myParent = e.Parent;

                if (myParent.HasValue && myParent.Value != e.Entity && myParent.Value == transform.Entity)
                {
                    e.IsMatrixCached &= !shouldRecalculate;
                    CascadeMatrixCalculation(e, all, transform);
                }
            }
        }
    }
}
