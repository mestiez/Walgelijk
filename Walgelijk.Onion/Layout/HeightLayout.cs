using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct HeightLayout : IConstraint
{
    public readonly float Height;

    public HeightLayout(float h)
    {
        Height = h;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate.MaxY = p.Instance.Rects.Intermediate.MinY + Height;
    }
}
