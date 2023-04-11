using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public struct HorizontalLayout : ILayout
{
    private float cursor;

    public void Apply(in ControlParams parent, int index, int childId)
    {
        var child = parent.Tree.EnsureInstance(childId);
        cursor += Onion.Theme.Padding;
        child.Rects.Intermediate = child.Rects.Intermediate.Translate(cursor, 0);
        cursor += child.Rects.Intermediate.Width;
    }
}