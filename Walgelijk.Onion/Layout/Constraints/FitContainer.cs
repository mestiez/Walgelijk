using Walgelijk.Onion.Controls;
using static Walgelijk.Onion.Layout.Resizable;

namespace Walgelijk.Onion.Layout;

public readonly struct FitContainer : IConstraint
{
    public readonly float? WidthRatio = 1;
    public readonly float? HeightRatio = 1;

    public FitContainer(float? widthRatio, float? heightRatio)
    {
        WidthRatio = widthRatio;
        HeightRatio = heightRatio;
    }

    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        var parent = p.Tree.EnsureInstance(p.Node.Parent.Identity);
        var padding = p.Theme.Padding * 2;

        if (WidthRatio.HasValue)
            p.Instance.Rects.Intermediate.Width = WidthRatio.Value * (parent.Rects.Intermediate.Width - padding);

        if (HeightRatio.HasValue)
            p.Instance.Rects.Intermediate.Height = HeightRatio.Value * (parent.Rects.Intermediate.Height - padding);
    }
}
