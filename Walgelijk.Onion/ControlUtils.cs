using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion;

public readonly struct ControlUtils
{
    public static void ProcessButtonLike(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        if (p.Instance.IsHover && p.Input.MousePrimaryHeld)
        {
            Onion.Navigator.FocusedControl = p.Instance.Identity;
            Onion.Navigator.ActiveControl ??= p.Instance.Identity;
        }

        if (p.Instance.IsActive && !p.Input.MousePrimaryHeld)
            Onion.Navigator.ActiveControl = null;
    }

    public static void ProcessDraggable(in ControlParams p, in Rect globalDraggableArea)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = globalDraggableArea;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        if (p.Instance.IsHover && p.Input.MousePrimaryPressed)
        {
            Onion.Navigator.FocusedControl = p.Instance.Identity;
            Onion.Navigator.ActiveControl ??= p.Instance.Identity;
        }

        if (p.Instance.IsActive)
        {
            p.Instance.Rects.Local = p.Instance.Rects.Local.Translate(p.Input.MouseDelta);

            if (p.Input.MousePrimaryRelease)
                Onion.Navigator.ActiveControl = null;
        }

    }

    public static void ProcessToggleLike(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        if (Onion.Navigator.ActiveControl != p.Instance.Identity && p.Instance.IsHover && p.Input.MousePrimaryRelease)
        {
            Onion.Navigator.FocusedControl = p.Instance.Identity;
            Onion.Navigator.ActiveControl = p.Instance.Identity;
        }
        else
        {
            //if (p.Instance.State.HasFlag(ControlState.Active) && p.Instance.HasFocus && p.Instance.State.HasFlag(ControlState.Hover) && p.Input.MousePrimaryRelease)
            //{
            ////    Onion.Navigator.ActiveControl = null;
            //}

            if (p.Instance.State.HasFlag(ControlState.Active) && !p.Instance.HasFocus)
                Onion.Navigator.ActiveControl = null;
        }
    }

    public static void Scrollable(in ControlParams p)
    {
        if (p.Instance.HasScroll)
            p.Instance.InnerScrollOffset += p.Input.ScrollDelta;
    }

    public static void ConsiderParentScroll(in ControlParams p)
    {
        if (p.Node.Parent != null && p.Tree.Instances.TryGetValue(p.Node.Parent.Identity, out var parent))
            p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(parent.InnerScrollOffset);
    }
}
