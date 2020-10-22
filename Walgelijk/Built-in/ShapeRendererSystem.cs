namespace Walgelijk
{
    /// <summary>
    /// System that renders components that implement <see cref="ShapeComponent"/>
    /// </summary>
    public class ShapeRendererSystem : System
    {
        public override void Render()
        {
            var basicShapes = Scene.GetAllComponentsOfType<ShapeComponent>();

            foreach (var pair in basicShapes)
            {
                RenderShape(pair);
            }
        }

        private void RenderShape(EntityWith<ShapeComponent> pair)
        {
            var shape = pair.Component;

            if (shape.Visible && shape.RenderTask.VertexBuffer != null)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(pair.Entity);
                shape.RenderTask.ScreenSpace = shape.ScreenSpace;
                shape.RenderTask.ModelMatrix = transform.LocalToWorldMatrix;
                RenderQueue.Add(shape.RenderTask, shape.RenderOrder);
            }
        }
    }
}
