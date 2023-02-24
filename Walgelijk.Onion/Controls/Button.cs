using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public struct Button : IControl
{
    public readonly string Label;

    public Button(string label)
    {
        Label = label;
    }

    public static bool Click(string label, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Button).GetHashCode(), identity, site), new Button(label));
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
        Draw.Colour = Colors.White;
        //Verzin een betere manier om data van Click() naar hier te brengen
        Draw.Text(Label, instance.Rects.Rendered.GetCenter(), Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Middle, instance.Rects.Rendered.Width);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}