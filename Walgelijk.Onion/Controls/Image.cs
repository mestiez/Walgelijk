using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Image : IControl
{
    private readonly IReadableTexture? texture;
    private readonly ImageContainmentMode containmentMode;

    public Image(IReadableTexture? texture, ImageContainmentMode containmentMode)
    {
        this.texture = texture;
        this.containmentMode = containmentMode;
    }

    public static void Start(IReadableTexture? texture, ImageContainmentMode containmentMode, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(Image).GetHashCode(), identity, site), new Image(texture, containmentMode));
        Onion.Tree.End();
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
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.Rendered;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.Rendered;
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        instance.Rects.Rendered = instance.Rects.ComputedGlobal;
        var t = node.GetAnimationTime();

        if (t <= float.Epsilon)
            return;

        var anim = instance.Animations;

        Draw.Colour = p.Theme.Image.Default;
        Draw.OutlineWidth = 0;
        anim.AnimateRect(ref instance.Rects.Rendered, t);
        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Image(texture ?? Texture.ErrorTexture, instance.Rects.Rendered, containmentMode, 0, p.Theme.Rounding);
    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}

