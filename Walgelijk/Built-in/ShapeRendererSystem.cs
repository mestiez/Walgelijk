using System.Numerics;

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
                var transform = Scene.GetComponentFast<TransformComponent>(pair.Entity);
                shape.RenderTask.ScreenSpace = shape.ScreenSpace;

                if (shape.HorizontalFlip || shape.VerticalFlip)
                {
                    float x = shape.HorizontalFlip ? -1 : 1;
                    float y = shape.VerticalFlip ? -1 : 1;
                    shape.RenderTask.ModelMatrix = Matrix3x2.CreateScale(x, y);
                    shape.RenderTask.ModelMatrix *= transform.LocalToWorldMatrix;
                }
                else
                    shape.RenderTask.ModelMatrix = transform.LocalToWorldMatrix;

                RenderQueue.Add(shape.RenderTask, shape.RenderOrder);
            }
        }
    }
}
