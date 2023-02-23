using Walgelijk.Onion.Controls;

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

        if (WidthRatio.HasValue)
            p.Instance.Rects.Intermediate.Width = WidthRatio.Value * parent.Rects.Intermediate.Width;
        if (HeightRatio.HasValue)
            p.Instance.Rects.Intermediate.Height = HeightRatio.Value * parent.Rects.Intermediate.Height;
    }
}

public readonly struct HorizontalLayout : ILayout
{
    public void Apply(in ControlParams p, int index, int childId)
    {
        var child = p.Tree.EnsureInstance(childId);
        child.Rects.Intermediate = child.Rects.Intermediate.Translate(0, index * 40);
    }
}