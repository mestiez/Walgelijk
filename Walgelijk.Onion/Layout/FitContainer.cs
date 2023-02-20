using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct FitContainer : ILayout
{
    public readonly float? WidthRatio = 1;
    public readonly float? HeightRatio = 1;

    public FitContainer(float? widthRatio, float? heightRatio)
    {
        WidthRatio = widthRatio;
        HeightRatio = heightRatio;
    }

    void ILayout.CalculateEither(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        var parent = p.ControlTree.EnsureInstance(p.Node.Parent.Identity);

        if (WidthRatio.HasValue)
            p.Instance.Rects.Target.Width = WidthRatio.Value * parent.Rects.Target.Width;
        if (HeightRatio.HasValue)
            p.Instance.Rects.Target.Height = HeightRatio.Value * parent.Rects.Target.Height;
    }
}