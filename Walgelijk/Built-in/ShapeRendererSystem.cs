using System.Numerics;

namespace Walgelijk;

/// <summary>
/// System that renders components that implement <see cref="ShapeComponent"/>
/// </summary>
public class ShapeRendererSystem : System
{
    public override void Render()
    {
        foreach (var sprite in Scene.GetAllComponentsOfType<SpriteComponent>())
            if (sprite.Visible)
                RenderSprite(sprite);

        foreach (var shape in Scene.GetAllComponentsOfType<ShapeComponent>())
            if (shape.Visible)
                RenderShape(shape);
    }

    private void RenderSprite(SpriteComponent sprite)
    {
        var transform = Scene.GetComponentFrom<TransformComponent>(sprite.Entity);
        sprite.RenderTask.ScreenSpace = sprite.ScreenSpace;

        if (sprite.HorizontalFlip || sprite.VerticalFlip)
        {
            float x = sprite.HorizontalFlip ? -1 : 1;
            float y = sprite.VerticalFlip ? -1 : 1;
            sprite.RenderTask.ModelMatrix = Matrix3x2.CreateScale(x, y);
            sprite.RenderTask.ModelMatrix *= transform.LocalToWorldMatrix;
        }
        else
            sprite.RenderTask.ModelMatrix = transform.LocalToWorldMatrix;

        sprite.RenderTask.Color = sprite.Color;

        RenderQueue.Add(sprite.RenderTask, sprite.RenderOrder);
    }

    private void RenderShape(ShapeComponent shape)
    {
        if (shape.RenderTask.VertexBuffer == null)
            return;

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

        shape.RenderTask.Color = shape.Color;

        RenderQueue.Add(shape.RenderTask, shape.RenderOrder);
    }
}
