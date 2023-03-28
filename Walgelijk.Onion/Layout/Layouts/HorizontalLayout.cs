using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public struct HorizontalLayout : ILayout
{
    private float cursor;

    public void Apply(in ControlParams parent, int index, int childId)
    {
        var child = parent.Tree.EnsureInstance(childId);
        child.Rects.Intermediate = child.Rects.Intermediate.Translate(cursor, 0);
        cursor += child.Rects.Intermediate.Width + Onion.Theme.Padding;
    }
}