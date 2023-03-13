using System.Runtime.CompilerServices;
using Walgelijk.Onion.Layout;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct DragWindow : IControl
{
    public readonly bool IsOpen;

    public DragWindow(bool isOpen)
    {
        IsOpen = isOpen;
    }

    public static ControlState Start(string title, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(DragWindow).GetHashCode(), identity, site), new DragWindow(true));
        instance.Name = title;
        return instance.State;
    }

    public static ControlState Start(string title, ref bool isOpen, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(DragWindow).GetHashCode(), identity, site), new DragWindow(isOpen));
        instance.Name = title;

        // Onion.Layout.Offset(5, 5);
        Onion.Layout.Size(18, 18);
        Onion.Layout.Enqueue(new StickTop());
        Onion.Layout.Enqueue(new StickRight());

        if (Button.Click("X", instance.Identity))
            isOpen = !isOpen;

        //if (!Openness.TryGetValue(instance.Identity, out isOpen))
        //    Openness.AddOrSet(identity, isOpen);
        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {

    }

    public void OnProcess(in ControlParams p)
    {
        if (IsOpen)
        {
            var d = p.Instance.Rects.ComputedGlobal;
            d.MaxY = d.MinY + 24 + Onion.Theme.Padding * 2;
            ControlUtils.ProcessDraggable(p, d);
        }
        else
        {
            p.Instance.CaptureFlags = CaptureFlags.None;
            p.Instance.Rects.Raycast = default;
            p.Instance.Rects.ComputedGlobal = default;
        }

        //if (p.Instance.IsActive && !wasActive)
        //{
        //    var topMost =
        //        (p.Node.Parent == null ? Onion.Tree.Root.GetChildren() : p.Node.Parent.GetChildren()).
        //        Min(static n => n.RequestedLocalOrder);
        //    p.Node.RequestedLocalOrder = topMost - 1;
        //}
    }

    public void OnRender(in ControlParams p)
    {
        if (!IsOpen)
            return;

        (ControlTree tree, Layout.Layout layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        instance.Rects.Rendered = instance.Rects.ComputedGlobal;

        var fg = Onion.Theme.Foreground;
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;

        if (instance.State.HasFlag(ControlState.Active))
            Draw.Colour = fg.Color.Brightness(0.9f);
        else if (instance.State.HasFlag(ControlState.Hover))
        {
            IControl.SetCursor(DefaultCursor.Pointer);
            Draw.Colour = fg.Color.Brightness(1.2f);
        }

        Draw.Quad(instance.Rects.Rendered, 0, Onion.Theme.Rounding);

        var bg = Onion.Theme.Background;
        Draw.Colour = fg.Color.Brightness(0.8f);
        Draw.Texture = fg.Texture;

        var container = instance.Rects.Rendered.Expand(-Onion.Theme.Padding);
        container.MinY += 24;
        Draw.Quad(container, 0, Onion.Theme.Rounding);
    }

    public void OnEnd(in ControlParams p)
    {

    }
}
