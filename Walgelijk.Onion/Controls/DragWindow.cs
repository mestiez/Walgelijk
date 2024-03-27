using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.Onion.Animations;
using Walgelijk.Onion.Assets;
using Walgelijk.Onion.Layout;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct DragWindow : IControl
{
    public static string CloseTooltipText = "Close";

    public readonly bool IsOpen;

    public DragWindow(bool isOpen)
    {
        IsOpen = isOpen;
    }

    [RequiresManualEnd]
    public static ControlState Start(string title, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(DragWindow).GetHashCode(), identity, site), new DragWindow(true));
        instance.Name = title;
        node.RequestedLocalOrder = Math.Max(1, node.RequestedLocalOrder); var padding = instance.Theme.Padding;
        return instance.State;
    }

    [RequiresManualEnd]
    public static ControlState Start(string title, ref bool isOpen, int identity = 0, [CallerLineNumber] int site = 0)
    {
        Onion.Animation.Add(new FadeAnimation());
        Onion.Animation.Add(new ShrinkAnimation(0.9f));
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(DragWindow).GetHashCode(), identity, site), new DragWindow(true));
        instance.Name = title;
        instance.Muted = true;
        instance.RenderFocusBox = false;

        float buttonSize = (instance.Theme.WindowTitleBarHeight[instance.State] - instance.Theme.Padding) / Onion.GlobalScale;
        Onion.Layout.Size(buttonSize, buttonSize).
            StickRight()
            .Move(instance.Theme.Padding / Onion.GlobalScale, -buttonSize - instance.Theme.Padding / Onion.GlobalScale);
        Ui.Decorators.Tooltip(CloseTooltipText);
        if (ImageButton.Click(BuiltInAssets.Icons.Exit, ImageContainmentMode.Cover, instance.Identity))
            isOpen = !isOpen;

        return instance.State;
    }

    public void OnAdd(in ControlParams p)
    {

    }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {
        var padding = p.Instance.Theme.Padding;
        p.Instance.Rects.InnerContentRectAdjustment = new Vector4(
            padding,
            padding + p.Instance.Theme.WindowTitleBarHeight[p.Instance.State],
            -padding,
            -padding
        );
    }

    public void OnProcess(in ControlParams p)
    {
        var d = p.Instance.Rects.ComputedGlobal;
        d.MaxY = d.MinY + p.Theme.WindowTitleBarHeight[p.Instance.State] + p.Theme.Padding * 2;

        ControlUtils.ProcessDraggable(p, d);

        if (p.Instance.IsNew || (p.Instance.IsActive && p.Input.MousePrimaryPressed && (p.Instance.Rects.Raycast?.ContainsPoint(p.Input.MousePosition) ?? false)))
        {
            var topMost = Onion.Tree.Nodes.Values.Max(static n => n.RequestedLocalOrder);
            p.Node.RequestedLocalOrder = topMost + 1;
        }
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        instance.Rects.Rendered = instance.Rects.ComputedGlobal;
        var t = node.GetAnimationTime();

        if (t <= float.Epsilon)
            return;

        var anim = instance.Animations;

        var fg = p.Theme.Foreground[instance.State];
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;
        Draw.ImageMode = fg.ImageMode;
        Draw.OutlineColour = p.Theme.OutlineColour[instance.State];
        Draw.OutlineWidth = p.Theme.OutlineWidth[instance.State];

        anim.AnimateRect(ref instance.Rects.Rendered, t);
        anim.AnimateColour(ref Draw.Colour, t);
        anim.AnimateColour(ref Draw.OutlineColour, t);
        Draw.Quad(instance.Rects.Rendered, 0, p.Theme.Rounding);

        Draw.Colour = p.Theme.Text[instance.State];
        anim.AnimateColour(ref Draw.Colour, t);

        var titleBarHeight = p.Theme.WindowTitleBarHeight[instance.State];
        var oldBounds = Draw.DrawBounds;
        Draw.DrawBounds = new DrawBounds(instance.Rects.ComputedDrawBounds with { MaxX = instance.Rects.ComputedDrawBounds.MaxX - p.Theme.Padding - titleBarHeight });
        Draw.Text(
            instance.Name, instance.Rects.Rendered.BottomLeft + new Vector2(p.Theme.Padding, p.Theme.Padding + 0.5f * titleBarHeight),
            Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Middle);
        Draw.DrawBounds = oldBounds;

        var bg = p.Theme.Background[instance.State];
        Draw.Colour = bg.Color;
        Draw.Texture = bg.Texture;
        Draw.ImageMode = bg.ImageMode;
        anim.AnimateColour(ref Draw.Colour, t);

        var container = instance.Rects.Rendered.Expand(-p.Theme.Padding);
        container.MinY += p.Theme.WindowTitleBarHeight[instance.State];
        anim.AnimateRect(ref container, t);
        Draw.OutlineWidth = 0;
        Draw.Quad(container, 0, p.Theme.Rounding);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}