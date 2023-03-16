using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public struct TextBoxOptions
{
    public string? Placeholder;
    public int? MaxLength;
    public Regex? Filter;
    public bool Password;
}

public readonly struct TextBox : IControl
{
    private record TextBoxState(bool IncomingChange);
    private static readonly Dictionary<int, TextBoxState> states = new();

    private static float TextOffset;
    private static int CursorIndex;
    private static float CursorPos;
    private static float slowTimer = 0;

    public static ControlState Create(ref string text, in TextBoxOptions options, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(TextBox).GetHashCode(), identity, site), new TextBox());
        instance.RenderFocusBox = true;
        Onion.Tree.End();
        if (states.TryGetValue(instance.Identity, out var state))
        {
            if (state.IncomingChange)
            {
                text = instance.Name;
                states[instance.Identity] = new(false);
            }
            else instance.Name = text;
        }
        else
        {
            instance.Name = text;
            states.Add(instance.Identity, new TextBoxState(false));
        }

        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {
    }

    public void OnProcess(in ControlParams p)
    {
        var hadFocus = p.Instance.HasFocus;
        ControlUtils.ProcessButtonLike(p);
        p.Instance.CaptureFlags |= CaptureFlags.Scroll;

        if (hadFocus != p.Instance.HasFocus)
            TextOffset = 0;

        if (p.Instance.IsActive)
        {
            slowTimer += p.GameState.Time.DeltaTime;

            var local = p.Input.MousePosition - p.Instance.Rects.ComputedGlobal.BottomLeft;
            //local.X -= Onion.Theme.Padding;
            local.X -= TextOffset;

            CursorIndex = GetIndexFromOffset(p.Instance.Name, local.X, out CursorPos);

            if (slowTimer > 0.1f)
            {
                slowTimer = 0;
                if (CursorPos < -TextOffset)
                    TextOffset += Onion.Theme.FontSize;
                else if (CursorPos > -TextOffset + p.Instance.Rects.Rendered.Width)
                    TextOffset -= Onion.Theme.FontSize;
            }
        }

        if (p.Instance.HasFocus)
        {
            if (p.Instance.HasScroll)
            {
                TextOffset += Onion.Input.ScrollDelta.Y;
                TextOffset = Utilities.Clamp(
                    TextOffset,
                    -Draw.CalculateTextWidth(p.Instance.Name) + p.Instance.Rects.Rendered.Width - Onion.Theme.Padding * 2, 0);
            }
        }
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.Layout layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var t = node.GetAnimationTime();
        var anim = instance.Animations;

        float d = instance.HasFocus ? TextOffset : 0;

        var fg = Onion.Theme.Foreground;
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;

        anim.AnimateRect(ref instance.Rects.Rendered, t);

        if (instance.IsHover)
            Draw.Colour = fg.Color.Brightness(1.2f);

        if (instance.IsActive)
            Draw.Colour = fg.Color.Brightness(0.9f);

        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Quad(instance.Rects.Rendered, 0, Onion.Theme.Rounding);

        Draw.Colour = Onion.Theme.Background.Color;
        Draw.Texture = Onion.Theme.Background.Texture;
        Draw.Quad(instance.Rects.Rendered.Expand(-2), 0, Onion.Theme.Rounding);

        Draw.ResetTexture();

        Draw.Font = Onion.Theme.Font;
        Draw.Colour = Onion.Theme.Text with { A = Draw.Colour.A };
        if (anim.ShouldRenderText(t))
        {
            if (!instance.HasFocus)
                Draw.Colour.A *= 0.5f;

            var ratio = instance.Rects.Rendered.Area / instance.Rects.ComputedGlobal.Area;
            var offset = new Vector2(instance.Rects.Rendered.MinX + Onion.Theme.Padding, 0.5f * (instance.Rects.Rendered.MinY + instance.Rects.Rendered.MaxY));
            offset.X += (int)d;
            Draw.Text(instance.Name, offset, new Vector2(ratio), HorizontalTextAlign.Left, VerticalTextAlign.Middle);

            if (instance.HasFocus && state.Time.SecondsSinceLoad % 1 > 0.3f)
            {
                var rect = new Rect(default, new Vector2(1, instance.Rects.Rendered.Height - Onion.Theme.Padding * 2)).Translate(
                    instance.Rects.ComputedGlobal.MinX + TextOffset,
                    (instance.Rects.ComputedGlobal.MinY + instance.Rects.ComputedGlobal.MaxY) / 2
                    );

                Draw.Quad(rect.Translate(CursorPos, 0), 0, 0);
            }
        }
    }

    public static int GetIndexFromOffset(ReadOnlySpan<char> input, float x, out float real)
    {
        real = 0;
        for (int i = 1; i <= input.Length; i++)
        {
            real = Draw.CalculateTextWidth(input[..i]);
            if (real >= x)
                return Math.Max(0, i);
        }
        real = Draw.CalculateTextWidth(input) + Draw.CalculateTextWidth(input[^1..^0]);
        return input.Length + 1;
    }

    public static float GetOffsetForChar(ReadOnlySpan<char> input, int index)
    {
        return Draw.CalculateTextWidth(input[..index]);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}
