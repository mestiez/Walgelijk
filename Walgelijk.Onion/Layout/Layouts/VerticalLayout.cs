using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public struct VerticalLayout : ILayout
{
    private float cursor;

    public void Apply(in ControlParams parent, int index, int childId)
    {
        var child = parent.Tree.EnsureInstance(childId);
        child.Rects.Intermediate = child.Rects.Intermediate.Translate(0, cursor);
        cursor += child.Rects.Intermediate.Height + Onion.Theme.Padding;
    }
}
