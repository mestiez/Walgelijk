using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Button : IControl
{
    public static bool Hold(string label, int identity = 0, [CallerLineNumber] int site = 0)
    {
        return CreateButton(label, identity, site).HasFlag(ControlState.Active);
    }

    public static bool Click(string label, int identity = 0, [CallerLineNumber] int site = 0)
    {
        return CreateButton(label, identity, site).HasFlag(ControlState.Hover) && Onion.Input.MousePrimaryPressed;
    }

    private static ControlState CreateButton(string label, int identity = 0, int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Button).GetHashCode(), identity, site), new Button());
        instance.RenderFocusBox = false;
        instance.Name = label;
        Onion.Tree.End();
        return instance.State;
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

        var animation = node.Alive ?
            Utilities.Clamp(node.SecondsAlive / instance.AllowedDeadTime) :
            1 - Utilities.Clamp(node.SecondsDead / instance.AllowedDeadTime);
        animation = Easings.Cubic.InOut(animation);

        instance.Rects.Rendered = instance.Rects.Rendered.Scale(Utilities.Lerp(animation, 1, 0.6f));

        var fg = Onion.Theme.Foreground;
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;

        if (instance.State.HasFlag(ControlState.Hover))
        {
            IControl.SetCursor(DefaultCursor.Pointer);
            Draw.Colour = fg.Color.Brightness(1.2f);
        }
        if (instance.State.HasFlag(ControlState.Active))
            Draw.Colour = fg.Color.Brightness(0.9f);

        Draw.Colour.A = (animation * animation * animation);
        Draw.Quad(instance.Rects.Rendered, 0, Onion.Theme.Rounding);
        Draw.ResetTexture();

        Draw.Font = Onion.Theme.Font;
        Draw.Colour = Onion.Theme.Text with { A = Draw.Colour.A };
        if (animation > 0.5f)
            Draw.Text(instance.Name, instance.Rects.Rendered.GetCenter(), Vector2.One, 
                HorizontalTextAlign.Center, VerticalTextAlign.Middle, instance.Rects.ComputedGlobal.Width);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}
