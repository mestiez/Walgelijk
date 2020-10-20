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
        public override void Update()
        {
            var pairs = Scene.GetAllComponentsOfType<TransformComponent>();

            //Parallel.ForEach(pairs, (pair) =>
            foreach (var pair in pairs)
            {
                if (!pair.Component.Parent.HasValue)
                    CascadeMatrixCalculation(pair, pairs);
            }//);
        }

        private void CascadeMatrixCalculation(EntityWith<TransformComponent> current, IEnumerable<EntityWith<TransformComponent>> collection, TransformComponent up = null)
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
