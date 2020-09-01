﻿namespace Walgelijk
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

        private void RenderShape(EntityWith<IShapeComponent> pair)
        {
            var shape = pair.Component;

            if (shape.RenderTask.VertexBuffer != null)
            {
#if DEBUG
                if (!Scene.TryGetComponentFrom<TransformComponent>(pair.Entity, out var transform))
                {
                    Logger.Error($"Attempt to render {nameof(IShapeComponent)} without {nameof(TransformComponent)}", nameof(ShapeRendererSystem));
                    return;
                }
# else
                var transform = Scene.GetComponentFrom<TransformComponent>(pair.Entity);
#endif
                shape.RenderTask.ScreenSpace = shape.ScreenSpace;
                shape.RenderTask.ModelMatrix = transform.LocalToWorldMatrix;
                RenderQueue.Add(shape.RenderTask, shape.RenderOrder);
            }
        }
    }
}
