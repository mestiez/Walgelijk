using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Group : IControl
{
    private readonly bool drawBackground;

    public Group(bool background)
    {
        this.drawBackground = background;
    }

    [RequiresManualEnd]
    public static void Start(bool background = true, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(Group).GetHashCode(), identity, site), new Group(background));
    }

    public void OnAdd(in ControlParams p)
    {
    }

    public void OnStart(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;
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

