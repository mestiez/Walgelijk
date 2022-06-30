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
        public bool Multithreading = false;

        /// <summary>
        /// Enable transform parenting?
        /// </summary>
        public bool Parenting = true;

        public override void Update()
        {
            var pairs = Scene.GetAllComponentsOfType<TransformComponent>();

            if (Multithreading)
                Parallel.ForEach(pairs, calc);
            else
                foreach (var pair in pairs)
                    calc(pair);

            void calc(EntityWith<TransformComponent> pair)
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
                transform.RecalculateModelMatrix(Matrix4x4.Identity);
        }

        private void CascadeMatrixCalculation(EntityWith<TransformComponent> current, IEnumerable<EntityWith<TransformComponent>> collection, TransformComponent? up = null)
        {
            var transform = current.Component;
            bool shouldRecalculate = !transform.IsMatrixCached;

            if (shouldRecalculate)
                transform.RecalculateModelMatrix(up?.LocalToWorldMatrix ?? Matrix4x4.Identity);

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
