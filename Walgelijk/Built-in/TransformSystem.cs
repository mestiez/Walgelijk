using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

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

        public override void Update()
        {
            var arr = ArrayPool<TransformComponent>.Shared.Rent(4096);
            var all = Scene.GetAllComponentsOfType(arr);

            foreach (var transform in all)
            {
                if (Parenting)
                    CascadeMatrixCalculation(transform, all);
                else if (!transform.Parent.HasValue)
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

        private void CascadeMatrixCalculation(TransformComponent transform, ReadOnlySpan<TransformComponent> collection, TransformComponent? up = null)
        {
            bool shouldRecalculate = !transform.IsMatrixCached;

            if (shouldRecalculate)
                transform.RecalculateModelMatrix(up?.LocalToWorldMatrix ?? Matrix3x2.Identity);

            foreach (var e in collection)
            {
                var myParent = e.Parent;

                if (myParent.HasValue && myParent.Value != e.Entity && myParent.Value == transform.Entity)
                {
                    e.IsMatrixCached &= !shouldRecalculate;
                    CascadeMatrixCalculation(e, collection, transform);
                }
            }
        }
    }
}
