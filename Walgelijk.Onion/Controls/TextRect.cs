using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct TextRect : IControl
{
    private readonly HorizontalTextAlign horizontal;
    private readonly VerticalTextAlign vertical;

    public TextRect(HorizontalTextAlign horizontal, VerticalTextAlign vertical)
    {
        this.horizontal = horizontal;
        this.vertical = vertical;
    }

    public static ControlState Create(string text, HorizontalTextAlign horizontal, VerticalTextAlign vertical, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(TextRect).GetHashCode(), identity, site), new TextRect(horizontal, vertical));
        instance.Name = text;
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
        p.Instance.CaptureFlags = CaptureFlags.None;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var t = node.GetAnimationTime();
        var anim = instance.Animations;

        instance.Rects.Rendered = instance.Rects.ComputedGlobal;

        Draw.Font = Onion.Theme.Font;
        Draw.Colour = Onion.Theme.Text[p.Instance.State];
        anim.AnimateColour(ref Draw.Colour, t);

        Vector2 pivot = instance.Rects.Rendered.GetCenter();

        switch (horizontal)
        {
            case HorizontalTextAlign.Left:
                pivot.X = instance.Rects.Rendered.MinX + Onion.Theme.Padding;
                break;
            case HorizontalTextAlign.Right:
                pivot.X = instance.Rects.Rendered.MaxX - Onion.Theme.Padding;
                break;
        }

        switch (vertical)
        {
            case VerticalTextAlign.Top:
                pivot.Y = instance.Rects.Rendered.MinY + Onion.Theme.Padding;
                break;
            case VerticalTextAlign.Bottom:
                pivot.Y = instance.Rects.Rendered.MaxY - Onion.Theme.Padding;
                break;
        }

        Draw.Text(instance.Name, pivot, Vector2.One, horizontal, vertical, instance.Rects.ComputedGlobal.Width - Onion.Theme.Padding * 2);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}
