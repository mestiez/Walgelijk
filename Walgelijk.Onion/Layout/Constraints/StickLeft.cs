using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct StickLeft : IConstraint
{
    public readonly bool Pad;

    public StickLeft(bool pad)
    {
        this.Pad = pad;
    }

    public void Apply(in ControlParams p)
    {
        var offset = 0 - p.Instance.Rects.Intermediate.MinX + (Pad ? p.Theme.Padding : 0);
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(offset, 0);
    }
}
