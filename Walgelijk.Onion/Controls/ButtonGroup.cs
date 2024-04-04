using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct ButtonGroup : IControl
{
    private readonly bool drawBackground;

    public ButtonGroup(bool background)
    {
        this.drawBackground = background;
    }

    [RequiresManualEnd]
    public static InteractionReport Start(bool background = true, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(ButtonGroup).GetHashCode(), identity, site), new ButtonGroup(background));
        instance.RenderFocusBox = false;
        return new InteractionReport(instance, node, InteractionReport.CastingBehaviour.Up);
    }

    public void OnAdd(in ControlParams p)
    {
    }

    public void OnStart(in ControlParams p)
    {

    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.Rects.Rendered = p.Instance.Rects.ComputedGlobal;

        ControlUtils.ProcessButtonLike(p);
    }

    public void OnRender(in ControlParams p)
    {
        if (!drawBackground)
            return;

        (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        instance.Rects.Rendered = instance.Rects.ComputedGlobal;
        var t = node.GetAnimationTime();

        if (t <= float.Epsilon)
            return;

        var anim = instance.Animations;

        var fg = p.Theme.Foreground[ControlState.None];
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;
        Draw.ImageMode = fg.ImageMode;
        Draw.OutlineColour = p.Theme.OutlineColour[ControlState.None];
        Draw.OutlineWidth = p.Theme.OutlineWidth[ControlState.None];

        anim.AnimateRect(ref instance.Rects.Rendered, t);
        anim.AnimateColour(ref Draw.Colour, t);
        anim.AnimateColour(ref Draw.OutlineColour, t);
        Draw.Quad(instance.Rects.Rendered, 0, p.Theme.Rounding);
    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}

