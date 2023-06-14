using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public struct VerticalLayout : ILayout
{
    private float cursor;

    public void Apply(in ControlParams p, int index, int childId)
    {
        var child = p.Tree.EnsureInstance(childId);
        cursor += p.Theme.Padding;
        child.Rects.Intermediate = child.Rects.Intermediate.Translate(0, cursor);
        cursor += child.Rects.Intermediate.Height;
    }
}
