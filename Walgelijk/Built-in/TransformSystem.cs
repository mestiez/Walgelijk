using System;

namespace Walgelijk
{
    /// <summary>
    /// The system responsible for processing transforms
    /// </summary>
    public class TransformSystem : System
    {
        public override void Initialise() { }

        public override void Render() { }

        public override void Update()
        {
            foreach (var pair in Scene.GetAllComponentsOfType<TransformComponent>())
            {
                var transform = pair.Component;
                if (!transform.IsMatrixCached)
                    transform.RecalculateModelMatrix();
            }
        }
    }
}
