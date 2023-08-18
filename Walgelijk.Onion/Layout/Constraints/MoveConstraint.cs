using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct MoveConstraint : IConstraint
{
    public readonly Vector2 Position;

    public MoveConstraint(float x, float y)
    {
        Position = new Vector2(x, y) * Onion.GlobalScale;
    }

    public MoveConstraint(Vector2 position)
    {
        Position = position;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(Position);
    }
}
