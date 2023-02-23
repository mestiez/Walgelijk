using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct SizeLayout : IConstraint
{
    public readonly Vector2 Size;

    public SizeLayout(float w, float h)
    {
        Size = new Vector2(w, h);
    }

    public SizeLayout(Vector2 size)
    {
        Size = size;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate.MaxX = p.Instance.Rects.Intermediate.MinX + Size.X;
        p.Instance.Rects.Intermediate.MinY = p.Instance.Rects.Intermediate.MaxY + Size.Y;
    }
}
