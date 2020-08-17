namespace Walgelijk
{
    /// <summary>
    /// The system responsible for processing transforms
    /// </summary>
    public class TransformSystem : ISystem
    {
        public Scene Scene { get; set; }

        public void Initialise() { }

        public void Execute()
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
