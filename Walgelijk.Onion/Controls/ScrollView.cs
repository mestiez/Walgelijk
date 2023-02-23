namespace Walgelijk.Onion.Controls;

public struct ScrollView : IControl
{
    public void OnAdd(in ControlParams p) { }


    public void OnStart(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Scroll | CaptureFlags.Hover;
    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        ControlUtils.Scrollable(p);
    }

    public void OnRender(in ControlParams p)
    {

    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}
