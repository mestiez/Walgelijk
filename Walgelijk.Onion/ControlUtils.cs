﻿using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion;

public readonly struct ControlUtils
{
    public static void ProcessButtonLike(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        if (p.Instance.IsHover)
        {
            IControl.SetCursor(DefaultCursor.Pointer);
            if (p.Input.MousePrimaryPressed)
            {
                Onion.Navigator.FocusedControl = p.Instance.Identity;
                Onion.Navigator.ActiveControl ??= p.Instance.Identity;
            }
        }

        if (p.Instance.IsActive && !p.Input.MousePrimaryHeld)
            Onion.Navigator.ActiveControl = null;
    }

    public static void ProcessDraggable(in ControlParams p, in Rect globalDraggableArea)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        var hoverDraggable = globalDraggableArea.ContainsPoint(p.Input.MousePosition) && p.Instance.IsHover;

        if (hoverDraggable)
        {
            IControl.SetCursor(DefaultCursor.Pointer);

            if (p.Input.MousePrimaryPressed)
            {
                Onion.Navigator.FocusedControl = p.Instance.Identity;
                Onion.Navigator.ActiveControl ??= p.Instance.Identity;
            }
        }

        if (p.Instance.IsActive)
        {
            p.Instance.Rects.Local = p.Instance.Rects.Local.Translate(p.Input.MouseDelta);
            if (p.Input.MousePrimaryRelease)
                Onion.Navigator.ActiveControl = null;
        }
    }

    public static void ProcessTriggerable(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        if (p.Instance.IsHover)
        {
            IControl.SetCursor(DefaultCursor.Pointer);

            if (p.Input.MousePrimaryPressed)
            {
                Onion.Navigator.FocusedControl = p.Instance.Identity;
                Onion.Navigator.ActiveControl = p.Instance.Identity;
            }

            if (p.Instance.IsActive && p.Input.MousePrimaryRelease)
            {
                if (p.Instance.IsTriggered)
                    Onion.Navigator.TriggeredControl = null;
                else
                    Onion.Navigator.TriggeredControl = p.Instance.Identity;
            }
        }

        if (p.Instance.IsActive && !p.Input.MousePrimaryHeld)
            Onion.Navigator.ActiveControl = null;
    }

    public static void Scrollable(in ControlParams p)
    {
        if (p.Instance.HasScroll)
        {
            var scrollBounds = p.Instance.Rects.ComputedScrollBounds;
            if (scrollBounds.Width > scrollBounds.Height)
                // we should prioritise scrolling horizontally
                p.Instance.InnerScrollOffset += p.Input.ScrollDelta.YX();
            else
                // we should prioritise scrolling vertically
                p.Instance.InnerScrollOffset += p.Input.ScrollDelta;
        }
    }

    public static void ConsiderParentScroll(in ControlParams p)
    {
        if (p.Node.Parent != null && p.Tree.Instances.TryGetValue(p.Node.Parent.Identity, out var parent))
            p.Instance.Rects.ComputedGlobal = p.Instance.Rects.ComputedGlobal.Translate(parent.InnerScrollOffset);
    }
}
