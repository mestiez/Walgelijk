using System.Runtime.CompilerServices;
using Walgelijk.Onion.Layout;

namespace Walgelijk.Onion.Controls;

public readonly struct Dummy : IControl
{
    public static void Start(int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Dummy).GetHashCode(), identity, site), new Dummy());
    }

    public void OnAdd(in ControlParams p)
    {
    }

    public void OnStart(in ControlParams p)
    {
        Onion.Layout.FitContainer(1, 1);
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
