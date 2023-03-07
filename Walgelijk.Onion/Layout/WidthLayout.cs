using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct WidthLayout : IConstraint
{
    public readonly float Width;

    public WidthLayout(float w)
    {
        Width = w;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate.MaxX = p.Instance.Rects.Intermediate.MinX + Width;
    }
}
