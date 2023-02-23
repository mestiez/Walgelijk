using System.Runtime.CompilerServices;
using Walgelijk.Onion.Layout;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public struct Dummy : IControl
{
    public void OnAdd(in ControlParams p)
    {
    }

    public void OnStart(in ControlParams p)
    {
        Onion.Layout.Position(0, 0);
        Onion.Layout.Constraints.Enqueue(new FitContainer(1, 1));
        p.Instance.Rects.Local = new Rect(0, 0, 1, 1);
        p.Instance.CaptureFlags = CaptureFlags.None;
        p.Instance.Rects.Raycast = null;
        p.Instance.Rects.DrawBounds = null;
    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.Rects.Rendered = p.Instance.Rects.ComputedGlobal;
    }

    public void OnRender(in ControlParams p)
    {

    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}

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
        p.Instance.Rects.DrawBounds = null;

        ControlUtils.Scrollable(p);
    }

    public void OnRender(in ControlParams p)
    {

    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}

public struct Button : IControl
{
    public static bool Click(string name, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Button).GetHashCode(), identity, site), new Button());
        Onion.Tree.End();
        return instance.State.HasFlag(ControlState.Hover) && Onion.Input.MousePrimaryPressed;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {
    }

    public void OnProcess(in ControlParams p)
    {
        ControlUtils.ProcessButtonLike(p);

        //if (p.Instance.State.HasFlag(ControlState.Scroll))
        //    p.Instance.InnerScrollOffset += Onion.Input.ScrollDelta; 
        //TODO dit moet ergens anders... 
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.Layout layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var animation = node.Alive ? Utilities.Clamp(node.SecondsAlive / instance.AllowedDeadTime) : 1 - Utilities.Clamp(node.SecondsDead / instance.AllowedDeadTime);
        animation = Easings.Cubic.InOut(animation);

        instance.Rects.Rendered = instance.Rects.Rendered.Scale(Utilities.Lerp(animation, 1, 0.6f));

        Draw.Colour = Colors.Red;
        if (instance.State.HasFlag(ControlState.Hover))
            Draw.Colour = Colors.Orange;
        if (instance.State.HasFlag(ControlState.Active))
            Draw.Colour = Colors.Red.Brightness(0.4f);

        Draw.Colour.A = (animation * animation * animation);
        Draw.Quad(instance.Rects.Rendered);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}