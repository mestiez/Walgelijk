using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

// YES THIS IS UNICODE VISUAL STUDIO JESUS CHRIST 🚞🚞🚠🌷🥀

public readonly struct AspectRatio : IConstraint 
{
    private readonly float heightOverWidth;
    private readonly Behaviour behaviour;

    public AspectRatio(float heightOverWidth, Behaviour behaviour = Behaviour.Grow)
    {
        this.heightOverWidth = heightOverWidth;
        this.behaviour = behaviour;
    }

    public enum Behaviour
    {
        Shrink,
        Grow
    }

    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        var w = p.Instance.Rects.Intermediate.Width;
        var h = p.Instance.Rects.Intermediate.Height;

        switch (behaviour)
        {
            case Behaviour.Shrink:
                if (w > h)
                    p.Instance.Rects.Intermediate.Width = h / heightOverWidth;
                else
                    p.Instance.Rects.Intermediate.Height = w * heightOverWidth;
                break;
            case Behaviour.Grow:
                if (w < h)
                    p.Instance.Rects.Intermediate.Width = h * heightOverWidth;
                else
                    p.Instance.Rects.Intermediate.Height = w / heightOverWidth;
                break;
        }
    }
}