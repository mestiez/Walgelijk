using System.Numerics;
using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Decorators;

public readonly struct Tooltip : IDecorator
{
    public readonly string Message;

    public Tooltip(string message)
    {
        Message = message;
    }

    public void RenderBefore(in ControlParams p)
    {
    }

    public void RenderAfter(in ControlParams p)
    {
        if (!p.Instance.IsHover || (p.GameState.Time.SecondsSinceSceneChangeUnscaled - p.Instance.LastStateChangeTime) < 1)
            return;

        const int cursorRadius = 8;

        var space = Game.Main.Window.Size;
        var point = p.Input.MousePosition;
        int padding = Onion.Theme.Padding;
        int padDbl = padding * 2;

        var rightPivot = point.X > space.X / 2;
        var bottomPivot = point.Y > space.Y / 2;

        if (rightPivot) 
            point.X -= cursorRadius;
        else
            point.X += cursorRadius;

        if (bottomPivot) 
            point.Y -= cursorRadius;
        else
            point.Y += cursorRadius;

        float freeHorizontalSpace = space.X - point.X - padding;
        float freeVerticalSpace = space.Y - point.Y - padding;

        if (rightPivot)
            freeHorizontalSpace = space.X - freeHorizontalSpace - padding;
        if (bottomPivot)
            freeVerticalSpace = space.Y - freeVerticalSpace - padding;

        var textWidth = Draw.CalculateTextWidth(Message);
        float rectWidth = MathF.Min(freeHorizontalSpace, textWidth + padDbl);

        var textHeight = Draw.CalculateTextHeight(Message, rectWidth - padding * 2);
        float rectHeight = MathF.Min(freeVerticalSpace, textHeight + padDbl);

        var appearance = Onion.Theme.Background[ControlState.None];
        var rect = new Rect(0, rectHeight, rectWidth, 0).Translate(
            rightPivot ? point.X - rectWidth : point.X,
            bottomPivot ? point.Y - rectHeight: point.Y);

        Draw.ResetDrawBounds();
        Draw.Order = Draw.Order.WithOrder(int.MaxValue);
        Draw.Colour = appearance.Color.WithAlpha(0.95f * p.Instance.GetTransitionProgress());
        //Draw.Colour = Colors.Red.WithAlpha(0.95f);
        Draw.Texture = appearance.Texture;
        Draw.OutlineWidth = 1;
        Draw.OutlineColour= Onion.Theme.Foreground[ControlState.None].Color;
        Draw.Quad(rect);
        Draw.OutlineWidth = 0;

        Draw.Colour = Onion.Theme.Text[ControlState.None];
        Draw.Text(Message, rect.TopLeft + new Vector2(padding), Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Top, rectWidth - padding * 2);
    }
}