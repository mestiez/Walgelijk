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
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Group).GetHashCode(), identity, site), new Group(background));
    }

    public void OnAdd(in ControlParams p)
    {
    }

    public void OnStart(in ControlParams p)
    {
        p.Instance.Rects.Local = new Rect(0, 0, 1, 1);
        p.Instance.CaptureFlags = CaptureFlags.None;
        p.Instance.Rects.Raycast = null;
        p.Instance.Rects.DrawBounds = null;
    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.Rects.Rendered = p.Instance.Rects.ComputedGlobal;
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.Rendered;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.Rendered;
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
        Draw.OutlineColour = p.Theme.OutlineColour[instance.State];
        Draw.OutlineWidth = p.Theme.OutlineWidth[instance.State];

        anim.AnimateRect(ref instance.Rects.Rendered, t);
        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Quad(instance.Rects.Rendered, 0, p.Theme.Rounding);
    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}

