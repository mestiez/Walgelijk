namespace Walgelijk
{
    /// <summary>
    /// System that renders basic shapes. Supports <see cref="IBasicShapeComponent"/>
    /// </summary>
    public class BasicRendererSystem : System
    {
        private Game Game => Scene.Game;

        public override void Initialise() { }

        public override void Update() { }

        public override void Render()
        {
            var basicShapes = Scene.GetAllComponentsOfType<IBasicShapeComponent>();

            foreach (var pair in basicShapes)
            {
                RenderShape(pair);
            }
        }

        private void RenderShape(ComponentEntityTuple<IBasicShapeComponent> pair)
        {
            var shape = pair.Component;

            if (shape.RenderTask.VertexBuffer != null)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(pair.Entity);
                var task = shape.RenderTask;
                task.ModelMatrix = transform.LocalToWorldMatrix;
                Game.RenderQueue.Enqueue(task);
            }
        }
    }
}
