﻿using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct RelativeLayout : ILayout
{
    public readonly Vector2? Position;
    public readonly Vector2? Size;

    public RelativeLayout(float x, float y)
    {
        Position = new Vector2(x, y);
        Size = null;
    }

    public RelativeLayout(Vector2? position, Vector2? size)
    {
        Position = position;
        Size = size;
    }

    public RelativeLayout(float x, float y, float w, float h)
    {
        Position = new Vector2(x, y);
        Size = new Vector2(w, h);
    }

    void ILayout.CalculateEither(in ControlParams p)
    {
        var parent = p.Node.Parent != null ? p.ControlTree.EnsureInstance(p.Node.Parent.Identity) : null;
        var offset = parent?.Rects.Target.BottomLeft ?? Vector2.Zero;

        if (Position.HasValue)
        {
            p.Instance.Rects.Target.MinX = offset.X + Position.Value.X;
            p.Instance.Rects.Target.MinY = offset.Y + Position.Value.Y;
        }

        if (Size.HasValue)
        {
            p.Instance.Rects.Target.Width = Size.Value.X;
            p.Instance.Rects.Target.Height = Size.Value.Y;
        }
    }
}