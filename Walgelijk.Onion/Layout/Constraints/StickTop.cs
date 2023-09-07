using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct StickTop : IConstraint
{
    public readonly bool Pad;

    public StickTop(bool pad)
    {
        this.Pad = pad;
    }

    public void Apply(in ControlParams p)
    {
        var offset = 0 - p.Instance.Rects.Intermediate.MinY + (Pad ? p.Theme.Padding : 0);
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(0, offset);
    }
}
   