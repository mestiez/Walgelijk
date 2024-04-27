using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct Inflate : IConstraint
{
    public readonly float Width;
    public readonly float Height;

    public Inflate(float w, float h)
    {
        Width = w * Onion.GlobalScale;
        Height = h * Onion.GlobalScale;
    }

    public void Apply(in ControlParams p)
    {
        ref var r = ref p.Instance.Rects.Intermediate;

        r.Width += Width;
        r.Height += Height;

        r = r.Translate(Width / -2, Height / -2);
    } //🍳
}
