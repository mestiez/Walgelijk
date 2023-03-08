using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion;

public readonly struct ControlUtils
{
    public static void ProcessButtonLike(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        if (p.Instance.State.HasFlag(ControlState.Hover) && p.Input.MousePrimaryHeld)
        {
            Onion.Navigator.FocusedControl = p.Instance.Identity;
            if (Onion.Navigator.ActiveControl == null)
                Onion.Navigator.ActiveControl = p.Instance.Identity;
        }

        if (p.Instance.State.HasFlag(ControlState.Active) && !p.Input.MousePrimaryHeld)
            Onion.Navigator.ActiveControl = null;
    }

    public static void ProcessToggleLike(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        if (Onion.Navigator.ActiveControl == null && p.Instance.State.HasFlag(ControlState.Hover) && p.Input.MousePrimaryRelease)
        {
            Onion.Navigator.FocusedControl = p.Instance.Identity;
            Onion.Navigator.ActiveControl = p.Instance.Identity;
        }
        else
        {
            if (p.Instance.State.HasFlag(ControlState.Active) && p.Instance.HasFocus && p.Instance.State.HasFlag(ControlState.Hover) && p.Input.MousePrimaryRelease)
            {
            //    Onion.Navigator.ActiveControl = null;
            }

            if (p.Instance.State.HasFlag(ControlState.Active) && !p.Instance.HasFocus)
                Onion.Navigator.ActiveControl = null;
        }
    }

    public static void Scrollable(in ControlParams p)
    {
        if (p.Instance.State.HasFlag(ControlState.Scroll))
            p.Instance.InnerScrollOffset += p.Input.ScrollDelta;
    }

    public static void ConsiderParentScroll(in ControlParams p)
    {
        if (p.Node.Parent != null && p.Tree.Instances.TryGetValue(p.Node.Parent.Identity, out var parent))
            p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(parent.InnerScrollOffset);
    }
}
