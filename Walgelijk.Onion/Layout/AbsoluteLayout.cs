﻿using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct AbsoluteLayout : ILayout
{
    public readonly Vector2? Position;
    public readonly Vector2? Size;

    public AbsoluteLayout(float x, float y)
    {
        Position = new Vector2(x, y);
        Size = null;
    }

    public AbsoluteLayout(Vector2? position, Vector2? size)
    {
        Position = position;
        Size = size;
    }

    public AbsoluteLayout(float x, float y, float w, float h)
    {
        Position = new Vector2(x, y);
        Size = new Vector2(w, h);
    }

    void ILayout.CalculateEither(in ControlParams p)
    {
        if (Position.HasValue)
        {
            p.Instance.Rects.Target.MinX = Position.Value.X;
            p.Instance.Rects.Target.MinY = Position.Value.Y;
        }

        if (Size.HasValue)
        {
            p.Instance.Rects.Target.Width = Size.Value.X;
            p.Instance.Rects.Target.Height = Size.Value.Y;
        }
    }
}