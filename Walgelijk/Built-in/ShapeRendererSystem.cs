namespace Walgelijk
{
    /// <summary>
    /// System that renders components that implement <see cref="IShapeComponent"/>
    /// </summary>
    public class ShapeRendererSystem : System
    {
        public override void Render()
        {
            var basicShapes = Scene.GetAllComponentsOfType<IShapeComponent>();

            foreach (var pair in basicShapes)
            {
                RenderShape(pair);
            }
        }

        private void RenderShape(ComponentEntityTuple<IShapeComponent> pair)
        {
            var shape = pair.Component;

            if (shape.RenderTask.VertexBuffer != null)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(pair.Entity);
                var task = shape.RenderTask;
                task.ScreenSpace = shape.ScreenSpace;
                task.ModelMatrix = transform.LocalToWorldMatrix;
                RenderQueue.Enqueue(task);
            }
        }
    }
}
