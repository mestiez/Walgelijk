using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct CenterInParent : IConstraint
{
    public readonly bool Horizontally;
    public readonly bool Vertically;

    public CenterInParent(bool horizontally, bool vertically)
    {
        Horizontally = horizontally;
        Vertically = vertically;
    }

    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        var parent = p.Tree.EnsureInstance(p.Node.Parent.Identity);
        var container = parent.Rects.Intermediate;
        var center = Vector2.Zero;
        var rect = p.Instance.Rects.Intermediate;

        if (Horizontally)
            center.X = container.Width / 2 - rect.Width / 2;

        if (Vertically)
            center.Y = container.Height / 2 - rect.Height / 2;

        p.Instance.Rects.Intermediate = rect.Translate(center);
    }
}
