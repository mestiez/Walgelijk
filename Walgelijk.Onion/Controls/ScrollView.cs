﻿using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct ScrollView : IControl
{
    public readonly bool BlockHover = true;
    public readonly bool Background = true;

    public ScrollView(bool blockHover, bool background)
    {
        BlockHover = blockHover;
        Background = background;
    }

    [RequiresManualEnd]
    public static void Start(bool background = false, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(ScrollView).GetHashCode(), identity, site), new ScrollView(true, background));
    }

    public void OnAdd(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Scroll;
        if (BlockHover)
            p.Instance.CaptureFlags |= CaptureFlags.Hover;
    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        ControlUtils.Scrollable(p);
    }

    public void OnRender(in ControlParams p)
    {
        if (Background)
        {
            (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

            instance.Rects.Rendered = instance.Rects.ComputedGlobal;
            var t = node.GetAnimationTime();

            if (t > 0)
            {
                var anim = instance.Animations;

                var fg = p.Theme.Foreground[ControlState.None];
                Draw.Colour = fg.Color;
                Draw.Texture = fg.Texture;
                Draw.ImageMode = fg.ImageMode;
                Draw.OutlineColour = p.Theme.OutlineColour[ControlState.None];
                Draw.OutlineWidth = p.Theme.OutlineWidth[ControlState.None];

                anim.AnimateRect(ref instance.Rects.Rendered, t);
                anim.AnimateColour(ref Draw.Colour, t);
                anim.AnimateColour(ref Draw.OutlineColour, t);
                Draw.Quad(instance.Rects.Rendered, 0, p.Theme.Rounding);
            }
        }

        //if (p.Instance.Rects.ChildContent.Height > p.Instance.Rects.Intermediate.Height)
        //{
        //    float height = p.Instance.Rects.Intermediate.Height * p.Instance.Rects.Intermediate.Height / p.Instance.Rects.ChildContent.Height;
        //    var max = p.Instance.Rects.Intermediate.Height - height;
        //    var offset = Utilities.Lerp(0, max, Utilities.MapRange(p.Instance.Rects.ComputedScrollBounds.MinY, p.Instance.Rects.ComputedScrollBounds.MaxY, 1, 0, p.Instance.InnerScrollOffset.Y));
        //    Draw.Colour = Onion.Theme.Accent.WithAlpha(1);
        //    Draw.Quad(new Rect(-2, 0, 0, height).Translate(p.Instance.Rects.ComputedGlobal.BottomRight).Translate(0, offset));
        //}

        //if (p.Instance.Rects.ChildContent.Width > p.Instance.Rects.Intermediate.Width)
        //{
        //    float width = p.Instance.Rects.Intermediate.Width * p.Instance.Rects.Intermediate.Width / p.Instance.Rects.ChildContent.Width;
        //    var max = p.Instance.Rects.Intermediate.Width - width;
        //    var offset = Utilities.Lerp(0, max, Utilities.MapRange(p.Instance.Rects.ComputedScrollBounds.MinX, p.Instance.Rects.ComputedScrollBounds.MaxX, 1, 0, p.Instance.InnerScrollOffset.X));
        //    Draw.Colour = Onion.Theme.Accent.WithAlpha(1);
        //    Draw.Quad(new Rect(0, -2, width, 0).Translate(p.Instance.Rects.ComputedGlobal.TopLeft).Translate(offset, 0));
        //}
    }

    public void OnEnd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }
}
