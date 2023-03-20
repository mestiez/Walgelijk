using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Checkbox : IControl
{
    private record CheckboxState(bool Value, bool IncomingChange);
    private static readonly Dictionary<int, CheckboxState> states = new();

    public static ControlState Create(ref bool value, string? label = null, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Checkbox).GetHashCode(), identity, site), new Checkbox());
        instance.RenderFocusBox = false;
        instance.Name = label ?? string.Empty;
        Onion.Tree.End();

        if (states.TryGetValue(instance.Identity, out var state))
        {
            if (state.IncomingChange)
                value = state.Value;
            states[instance.Identity] = new(value, false);
        }
        else
            states.Add(instance.Identity, new CheckboxState(value, false));

        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnStart(in ControlParams p) { }

    public void OnProcess(in ControlParams p)
    {
        ControlUtils.ProcessButtonLike(p);
        if (p.Instance.IsActive && p.Input.MousePrimaryPressed)
        {
            var s = states[p.Identity];
            states[p.Identity] = new CheckboxState(!s.Value, true);
        }
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

        var checkBoxRect = instance.Rects.Rendered with { Width = instance.Rects.Rendered.Height };
        var textBoxRect = instance.Rects.Rendered.Translate(checkBoxRect.Width, 0) with
        { Width = instance.Rects.Rendered.Width - checkBoxRect.Width };

        anim.AnimateRect(ref checkBoxRect, t);
        anim.AnimateRect(ref textBoxRect, t);

        if (instance.State.HasFlag(ControlState.Hover))
            Draw.Colour = fg.Color.Brightness(1.2f);
        if (instance.State.HasFlag(ControlState.Active))
            Draw.Colour = fg.Color.Brightness(0.9f);

        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Quad(checkBoxRect, 0, Onion.Theme.Rounding);

        if (states[p.Identity].Value)
        {
            Draw.Colour = Onion.Theme.Accent;
            anim.AnimateColour(ref Draw.Colour, t);
            Draw.Quad(checkBoxRect.Expand(-6), 0, Onion.Theme.Rounding);
        }

        Draw.ResetTexture();
        Draw.Font = Onion.Theme.Font;
        Draw.Colour = Onion.Theme.Text with { A = Draw.Colour.A };
        if (anim.ShouldRenderText(t))
        {
            var ratio = instance.Rects.Rendered.Area / instance.Rects.ComputedGlobal.Area;
            Draw.Text(instance.Name, textBoxRect.GetCenter(), new Vector2(ratio),
                HorizontalTextAlign.Center, VerticalTextAlign.Middle, textBoxRect.Width);
        }
    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}
