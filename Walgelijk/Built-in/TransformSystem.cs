using System;
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

        private readonly EntityWith<TransformComponent>[] transforms = new EntityWith<TransformComponent>[4096];

        public override void Update()
        {
            var pairs = Scene.GetAllComponentsOfType(transforms);

            //if (Multithreading)
            //    Parallel.ForEach(pairs, calc);
            //else
            foreach (var pair in pairs)
            {
                if (Parenting)
                    CascadeMatrixCalculation(pair, pairs);
                else if (!pair.Component.Parent.HasValue)
                    CalculateMatrix(pair);
            }
        }

        private void CalculateMatrix(EntityWith<TransformComponent> pair)
        {
            var transform = pair.Component;
            bool shouldRecalculate = !transform.IsMatrixCached;

            if (shouldRecalculate)
                transform.RecalculateModelMatrix(Matrix3x2.Identity);
        }

        private void CascadeMatrixCalculation(EntityWith<TransformComponent> current, ReadOnlySpan<EntityWith<TransformComponent>> collection, TransformComponent? up = null)
        {
            var transform = current.Component;
            bool shouldRecalculate = !transform.IsMatrixCached;

            if (shouldRecalculate)
                transform.RecalculateModelMatrix(up?.LocalToWorldMatrix ?? Matrix3x2.Identity);

            foreach (var e in collection)
            {
                var myParent = e.Component.Parent;

                if (myParent.HasValue && myParent.Value != e.Entity && myParent.Value == current.Entity)
                {
                    e.Component.IsMatrixCached &= !shouldRecalculate;
                    CascadeMatrixCalculation(e, collection, current.Component);
                }
            }
        }
    }
}
