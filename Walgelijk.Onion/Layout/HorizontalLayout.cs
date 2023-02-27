using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct HorizontalLayout : ILayout
{
    public void Apply(in ControlParams p, int index, int childId)
    {
        var child = p.Tree.EnsureInstance(childId);
        if (index > 0)
        {
            var widthSoFar = p.Node.GetChildren().Take(index).Sum(static i => Onion.Tree.EnsureInstance(i.Identity).Rects.Intermediate.Width + 5);
            child.Rects.Intermediate = child.Rects.Intermediate.Translate(widthSoFar, 0);
        }
    }
}