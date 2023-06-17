using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct FitContainer : IConstraint
{
    public readonly float? WidthRatio = 1;
    public readonly float? HeightRatio = 1;
    public readonly bool Pad;

    public FitContainer(float? widthRatio, float? heightRatio, bool pad = true)
    {
        WidthRatio = widthRatio;
        HeightRatio = heightRatio;
        Pad = pad;
    }

    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        var parent = p.Tree.EnsureInstance(p.Node.Parent.Identity).Rects.GetInnerContentRect();
        var padding = Pad ? p.Theme.Padding * 2 : 0;

        if (WidthRatio.HasValue)
            p.Instance.Rects.Intermediate.Width = WidthRatio.Value * (parent.Width - padding);

        if (HeightRatio.HasValue)
            p.Instance.Rects.Intermediate.Height = HeightRatio.Value * (parent.Height - padding);
    }
}
