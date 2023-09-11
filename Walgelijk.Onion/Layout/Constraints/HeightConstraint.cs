using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct HeightConstraint : IConstraint
{
    public readonly float Height;

    public HeightConstraint(float h)
    {
        Height = h * Onion.GlobalScale;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate.MaxY = p.Instance.Rects.Intermediate.MinY + Height;
    }
}
