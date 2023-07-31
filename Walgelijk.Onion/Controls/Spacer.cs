using System.Runtime.CompilerServices;

namespace Walgelijk.Onion.Controls;

public readonly struct Spacer : IControl
{
    /// <summary>
    /// A spacing control. Does nothing, renders nothing, just has occupies space.
    /// </summary>
    public static void Start(float width, float height, int identity = 0, [CallerLineNumber] int site = 0)
    {
        Onion.Layout.Size(width, height);
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(Spacer).GetHashCode(), identity, site), new Spacer());
        Onion.Tree.End();
    }

    /// <summary>
    /// A spacing control. Does nothing, renders nothing, just has occupies space.
    /// </summary>
    public static void Start(float size, int identity = 0, [CallerLineNumber] int site = 0)
    {
        Onion.Layout.Size(size, size);
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(Spacer).GetHashCode(), identity, site), new Spacer());
        Onion.Tree.End();
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
    }

    public void OnRender(in ControlParams p)
    {
    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}
