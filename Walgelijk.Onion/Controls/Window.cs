using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct DragWindow : IControl
{
    public static ControlState Start(string title, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(DragWindow).GetHashCode(), identity, site), new DragWindow());
        instance.Name = title;
        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {

    }

    public void OnProcess(in ControlParams p)
    {
        //var d = p.Instance.Rects.ComputedGlobal;
        //d.MaxY = d.MinY + 32;
        ControlUtils.ProcessDraggable(p, p.Instance.Rects.ComputedGlobal);
    }

    public void OnRender(in ControlParams p)
    {
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
