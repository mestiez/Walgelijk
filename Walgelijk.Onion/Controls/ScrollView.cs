using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public struct ScrollView : IControl
{
    public void OnAdd(in ControlParams p) { }


    public void OnStart(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Scroll;
    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        ControlUtils.Scrollable(p);
    }

    public void OnRender(in ControlParams p)
    {
        //TODO dit werkt niet
        //float height = p.Instance.Rects.Intermediate.Height * p.Instance.Rects.Intermediate.Height / p.Instance.Rects.ChildContent.Height;

        //var max = p.Instance.Rects.Intermediate.Height - height;
        //var offset = Utilities.Lerp(0, max, Utilities.MapRange(p.Instance.Rects.ChildContent.MinY, p.Instance.Rects.ChildContent.MaxY, 1, 0, p.Instance.InnerScrollOffset.Y));

        //Draw.Colour = Onion.Theme.Accent.WithAlpha(0.5f);
        //Draw.Quad(new Rect(-2,0,0, height).Translate(p.Instance.Rects.Intermediate.BottomRight).Translate(0, offset));
    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}
