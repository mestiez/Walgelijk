using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public struct TextBoxOptions
{
    public string? Placeholder;
    public int? MaxLength;
    public Regex? Filter;
    public bool Password;
}

public readonly struct TextBox : IControl
{
    public static ControlState Create(ref string label, in TextBoxOptions options, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(TextBox).GetHashCode(), identity, site), new Button());
        instance.RenderFocusBox = true;
        instance.Name = label;
        Onion.Tree.End();
        return instance.State;
        //TODO maak alles
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

        var t = node.GetAnimationTime();
        var anim = instance.Animations;

        var fg = Onion.Theme.Foreground;
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;

        anim.AnimateRect(ref instance.Rects.Rendered, t);

        if (instance.State.HasFlag(ControlState.Hover))
            Draw.Colour = fg.Color.Brightness(1.2f);
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
