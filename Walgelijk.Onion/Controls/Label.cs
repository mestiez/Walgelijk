using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Label : IControl
{
    private readonly HorizontalTextAlign horizontal;

    public Label(HorizontalTextAlign horizontal)
    {
        this.horizontal = horizontal;
    }

    public static void Create(string text, HorizontalTextAlign horizontal = HorizontalTextAlign.Left, int identity = 0, [CallerLineNumber] int site = 0)
    {
        Onion.Layout.PreferredSize();
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(Label).GetHashCode(), identity, site), new Label(horizontal));
        instance.Name = text;
        Onion.Tree.End();
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

        if (!anim.ShouldRenderText(t))
            return;

        Draw.Font = p.Theme.Font;
        Draw.Colour = p.Theme.Text[p.Instance.State];
        anim.AnimateColour(ref Draw.Colour, t);
        anim.AnimateRect(ref instance.Rects.Rendered, t);

        Vector2 pivot = instance.Rects.Rendered.GetCenter();

        switch (horizontal)
        {
            case HorizontalTextAlign.Left:
                pivot.X = instance.Rects.Rendered.MinX + p.Theme.Padding;
                break;
            case HorizontalTextAlign.Right:
                pivot.X = instance.Rects.Rendered.MaxX - p.Theme.Padding;
                break;
        }

        Draw.Text(instance.Name, pivot, Vector2.One, horizontal, VerticalTextAlign.Middle);
        //instance.PreferredHeight = p.Theme.Font.Size;
        instance.PreferredWidth = Draw.CalculateTextWidth(instance.Name) + p.Theme.Padding * 2;
        instance.PreferredHeight = Draw.CalculateTextHeight(instance.Name, instance.PreferredWidth ?? float.PositiveInfinity);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}
