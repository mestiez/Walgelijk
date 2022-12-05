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
            foreach (var pair in Scene.GetAllComponentsOfType<ShapeComponent>())
            {
                RenderShape(pair);
            }
        }

        private void RenderShape(ShapeComponent shape)
        {
            if (shape.Visible && shape.RenderTask.VertexBuffer != null)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(shape.Entity);
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
