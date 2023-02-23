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
        p.Instance.Rects.Intermediate.MinX = Position.X;
        p.Instance.Rects.Intermediate.MinY = Position.Y;
    }
}
