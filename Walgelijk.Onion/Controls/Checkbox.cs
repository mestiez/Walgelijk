using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.Onion.Assets;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Checkbox : IControl
{
    private record CheckboxState(bool Value, bool IncomingChange);
    private static readonly Dictionary<int, CheckboxState> states = new();

    public static InteractionReport Create(ref bool value, string? label = null, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(Checkbox).GetHashCode(), identity, site), new Checkbox());
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

        return new InteractionReport(instance, node, InteractionReport.CastingBehaviour.Up);
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
        (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var t = node.GetAnimationTime();
        var anim = instance.Animations;

        var fg = p.Theme.Foreground[instance.State];
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;
        Draw.ImageMode = fg.ImageMode;
        Draw.OutlineColour = p.Theme.OutlineColour[instance.State];
        Draw.OutlineWidth = p.Theme.OutlineWidth[instance.State];

        anim.AnimateRect(ref instance.Rects.Rendered, t);

        var checkBoxRect = instance.Rects.Rendered with { Width = instance.Rects.Rendered.Height };
        var textBoxRect = instance.Rects.Rendered.Translate(checkBoxRect.Width, 0) with { Width = instance.Rects.Rendered.Width - checkBoxRect.Width };

        anim.AnimateRect(ref checkBoxRect, t);
        anim.AnimateRect(ref textBoxRect, t);

        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Quad(checkBoxRect, 0, p.Theme.Rounding);

        if (states[p.Identity].Value)
        {
            Draw.ImageMode = default;
            Draw.OutlineWidth = 0;
            Draw.Colour = p.Theme.Accent[p.Instance.State];
            anim.AnimateColour(ref Draw.Colour, t);
            Draw.Colour.A *= 0.1f;
            Draw.Quad(checkBoxRect, 0, p.Theme.Rounding);
            Draw.Colour.A /= 0.1f;
            Draw.Image(BuiltInAssets.Icons.Check, checkBoxRect.Expand(-p.Theme.Padding / 4), ImageContainmentMode.Contain, 0, p.Theme.Rounding);
        }

        Draw.ResetTexture();
        Draw.Font = p.Theme.Font;
        Draw.Colour = p.Theme.Text[instance.State];
        anim.AnimateColour(ref Draw.Colour, t);
        if (anim.ShouldRenderText(t))
        {
            var ratio = instance.Rects.Rendered.Area / instance.Rects.ComputedGlobal.Area;
            var c = (textBoxRect.BottomLeft + textBoxRect.TopLeft) / 2;
            c.X += p.Theme.Padding;
            Draw.Text(instance.Name, c, new Vector2(ratio), HorizontalTextAlign.Left, VerticalTextAlign.Middle, textBoxRect.Width);
        }
    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}
