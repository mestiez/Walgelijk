﻿using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct TextRect : IControl
{
    private readonly HorizontalTextAlign horizontal;
    private readonly VerticalTextAlign vertical;

    public TextRect(HorizontalTextAlign horizontal, VerticalTextAlign vertical)
    {
        this.horizontal = horizontal;
        this.vertical = vertical;
    }

    public static ControlState Create(string text, HorizontalTextAlign horizontal, VerticalTextAlign vertical, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(TextRect).GetHashCode(), identity, site), new TextRect(horizontal, vertical));
        instance.Name = text;
        Onion.Tree.End();
        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {
    }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.None;
        p.Instance.Rects.Rendered = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.Rendered;
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var t = node.GetAnimationTime();
        var anim = instance.Animations;

        if (!anim.ShouldRenderText(t))
            return;

        instance.Rects.Rendered = instance.Rects.ComputedGlobal;
        bool hide = instance.Rects.Rendered.Area <= float.Epsilon;

        Draw.Font = p.Theme.Font;
        Draw.Colour = p.Theme.Text[p.Instance.State];
        int wrapWidth = (int)instance.Rects.ComputedGlobal.Width - p.Theme.Padding * 2;

        if (!hide)
        {
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

            switch (vertical)
            {
                case VerticalTextAlign.Top:
                    pivot.Y = instance.Rects.Rendered.MinY + p.Theme.Padding;
                    break;
                case VerticalTextAlign.Bottom:
                    pivot.Y = instance.Rects.Rendered.MaxY - p.Theme.Padding;
                    break;
            }
            Draw.Text(instance.Name, pivot, Vector2.One, horizontal, vertical, wrapWidth);
        }

        instance.PreferredHeight = Draw.CalculateTextHeight(instance.Name, wrapWidth) + p.Theme.Padding * 2;
    }

    public void OnEnd(in ControlParams p)
    {
    }
}
