using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct PositionLayout : IConstraint
{
    public readonly Vector2 Position;

    public PositionLayout(float x, float y)
    {
        Position = new Vector2(x, y);
    }

    public PositionLayout(Vector2 position)
    {
        Position = position;
    }

    public void Apply(in ControlParams p)
    {
        // TODO hij moet heel de control meebewegen want nu rekt hij het gewoon uit, niet goed
        var offsetX = Position.X + p.Instance.Rects.Intermediate.MinX;
        var offsetY = Position.Y + p.Instance.Rects.Intermediate.MinY;

        p.Instance.Rects.Intermediate.MinX += offsetX;
        p.Instance.Rects.Intermediate.MinY += offsetY;

        p.Instance.Rects.Intermediate.MaxX += offsetX;
        p.Instance.Rects.Intermediate.MaxY += offsetY;
    }
}
