using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.Onion.Animations;
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

        var t = node.SecondsAlive / instance.AllowedDeadTime;
        var anim = instance.Animations;

        var fg = Onion.Theme.Foreground;
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;

        anim.AnimateRect(ref instance.Rects.Rendered, t);

        if (instance.State.HasFlag(ControlState.Hover))
        {
            IControl.SetCursor(DefaultCursor.Pointer);
            Draw.Colour = fg.Color.Brightness(1.2f);
        }
        if (instance.State.HasFlag(ControlState.Active))
            Draw.Colour = fg.Color.Brightness(0.9f);

        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Quad(instance.Rects.Rendered, 0, Onion.Theme.Rounding);
        Draw.ResetTexture();

        Draw.Font = Onion.Theme.Font;
        Draw.Colour = Onion.Theme.Text with { A = Draw.Colour.A };
        if (anim.ShouldRenderText(t))
        {
            var ratio = instance.Rects.Rendered.Area / instance.Rects.ComputedGlobal.Area;
            Draw.Text(instance.Name, instance.Rects.Rendered.GetCenter(), new Vector2(ratio),
                HorizontalTextAlign.Center, VerticalTextAlign.Middle, instance.Rects.ComputedGlobal.Width);
        }
    }

    public void OnEnd(in ControlParams p)
    {
    }
}
