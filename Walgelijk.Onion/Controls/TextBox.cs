using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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

    private static float textOffset;
    private static int cursorIndex;
    private static float cursorPosition;
    private static float slowTimer = 0;
    private static (int MinInc, int MaxExc)? selection;
    private static float cursorBlinkTimer;
    private static int selectionInitialIndex //TODO want je weet wel als je sleep select doet
    private static readonly TextMeshGenerator.ColourInstruction[] textColourInstructions = new TextMeshGenerator.ColourInstruction[3];

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
        {
            if (hadFocus)
                selection = null;
            textOffset = 0;
        }

        if (p.Instance.IsActive)
        {
            if (p.Input.MousePrimaryPressed)
                selection = null;

            //we dont use MoveCursor here because this is a special case where the mouse input determines the cursor position
            slowTimer += p.GameState.Time.DeltaTime;
            cursorBlinkTimer = 0;

            var local = p.Input.MousePosition - p.Instance.Rects.ComputedGlobal.BottomLeft;
            local.X -= textOffset;
            local.X -= Onion.Theme.Padding;

            cursorIndex = OffsetToIndex(p.Instance.Name, local.X, out cursorPosition);

            if (selection.HasValue)
            {
                if (cursorIndex < selection.Value.MinInc)
                    selection = (cursorIndex, selection.Value.MaxExc);
                else if (cursorIndex > selection.Value.MaxExc)
                    selection = (selection.Value.MinInc, cursorIndex);
            }
            else
                selection = (cursorIndex, cursorIndex);

            if (slowTimer > 0.05f)
            {
                slowTimer = 0;
                if (cursorPosition < -textOffset)
                    textOffset += Onion.Theme.FontSize;
                else if (cursorPosition > -textOffset + p.Instance.Rects.Rendered.Width)
                    textOffset -= Onion.Theme.FontSize;
            }
        }

        if (p.Instance.HasFocus)
        {
            cursorBlinkTimer += p.GameState.Time.DeltaTime;

            p.Instance.CaptureFlags |= CaptureFlags.Key;

            if (p.Instance.HasKey)
                ProcessKeyInput(p);

            if (p.Instance.HasScroll)
            {
                var textWidth = Draw.CalculateTextWidth(p.Instance.Name);
                var maxWidth = p.Instance.Rects.ComputedGlobal.Width - Onion.Theme.Padding * 2;

                if (textWidth > maxWidth)
                {
                    textOffset += Onion.Input.ScrollDelta.Y;
                    textOffset = Utilities.Clamp(
                        textOffset,
                        -textWidth + maxWidth, 0);
                }
                else
                    textOffset = 0;
            }
        }
        else
            p.Instance.CaptureFlags &= ~CaptureFlags.Key;
    }

    private static void ProcessKeyInput(in ControlParams p)
    {
        // cursor movement with directional keys
        var dir = (int)(p.Input.DirectionKeyReleased.X > float.Epsilon ? 1 : (p.Input.DirectionKeyReleased.X < -float.Epsilon ? -1 : 0));
        if (dir != 0)
        {
            if (p.Input.ShiftHeld)
            {
                if (selection.HasValue)
                    selection = (
                        Math.Min(cursorIndex + dir, Math.Min(selection.Value.MinInc, selection.Value.MaxExc)),
                        Math.Max(cursorIndex + dir, Math.Max(selection.Value.MinInc, selection.Value.MaxExc)));
                else
                    selection = (Math.Min(cursorIndex, cursorIndex + dir), Math.Max(cursorIndex, cursorIndex + dir));
                MoveCursor(p, cursorIndex + dir);
            }
            else
            {
                if (selection.HasValue)
                    MoveCursor(p, dir > 0 ? selection.Value.MaxExc : selection.Value.MinInc);
                else
                    MoveCursor(p, cursorIndex + dir);
                selection = null;
            }
        }

        if (p.Input.CtrlHeld && p.Input.AlphanumericalHeld.Contains(Key.A))
        {
            selection = (0, p.Instance.Name.Length);
        }

        // process input text
        if (p.Input.TextEntered.Length > 0)
        {
            string textToAdd = string.Empty;
            int backspaceCount = 0;
            int deleteCount = 0;
            for (int i = 0; i < p.Input.TextEntered.Length; i++)
            {
                var c = p.Input.TextEntered[i];
                switch (c)
                {
                    case (char)0x7F:
                        deleteCount++;
                        break;
                    case '\b':
                        backspaceCount++;
                        break;
                    default:
                        if (!char.IsControl(c))
                            textToAdd += c;
                        break;
                }
            }

            if (textToAdd.Length > 0)
                AppendText(p, textToAdd);
            for (int i = 0; i < backspaceCount; i++)
                Backspace(p);
        }

        // process delete
        if (p.Input.DeletePressed)
            Delete(p);
    }

    private static void AppendText(in ControlParams p, ReadOnlySpan<char> text)
    {
        DeleteSelection(p);

        p.Instance.Name = p.Instance.Name.Insert(cursorIndex, new string(p.Input.TextEntered.Where(static c => !char.IsControl(c)).ToArray()));
        states[p.Instance.Identity] = new TextBoxState(true);
        MoveCursor(p, cursorIndex + p.Input.TextEntered.Length);
    }

    private static void Backspace(in ControlParams p)
    {
        if (DeleteSelection(p))
            return;

        if (cursorIndex < 1)
            return;

        MoveCursor(p, cursorIndex - 1);
        Delete(p);
    }

    private static void Delete(in ControlParams p)
    {
        if (DeleteSelection(p))
            return;

        if (cursorIndex >= p.Instance.Name.Length)
            return;

        p.Instance.Name = p.Instance.Name[..cursorIndex] + p.Instance.Name[(cursorIndex + 1)..];
        states[p.Instance.Identity] = new TextBoxState(true);
    }

    private static bool DeleteSelection(in ControlParams p)
    {
        if (!selection.HasValue || selection.Value.MinInc >= selection.Value.MaxExc)
            return false;

        p.Instance.Name = p.Instance.Name.Remove(selection.Value.MinInc, (selection.Value.MaxExc - selection.Value.MinInc));
        states[p.Instance.Identity] = new TextBoxState(true);
        MoveCursor(p, selection.Value.MinInc);
        selection = null;
        textOffset = 0;
        return true;
    }

    private static void MoveCursor(in ControlParams p, int targetIndex)
    {
        cursorIndex = targetIndex;
        cursorPosition = IndexToOffset(p.Instance.Name, cursorIndex);
        cursorIndex = Utilities.Clamp(cursorIndex, 0, p.Instance.Name.Length);

        if (cursorPosition < -textOffset)
            textOffset += Onion.Theme.FontSize;
        else if (cursorPosition > -textOffset + p.Instance.Rects.Rendered.Width)
            textOffset -= Onion.Theme.FontSize;
        cursorBlinkTimer = 0;
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.Layout layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var t = node.GetAnimationTime();
        var anim = instance.Animations;

        float d = instance.HasFocus ? textOffset : 0;

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

            var col = Draw.Colour;

            var ratio = instance.Rects.Rendered.Area / instance.Rects.ComputedGlobal.Area;
            var offset = new Vector2(instance.Rects.Rendered.MinX + Onion.Theme.Padding, 0.5f * (instance.Rects.Rendered.MinY + instance.Rects.Rendered.MaxY));
            offset.X += (int)d;

            bool drawSelectionTextColour = false;
            if (p.Instance.HasFocus && selection.HasValue && selection.Value.MinInc < selection.Value.MaxExc)
            {
                Draw.Colour = (Vector4.One - fg.Color) with { W = col.A * 0.5f };
                var selRect = instance.Rects.Rendered;

                var m = selRect.MinX + textOffset;
                selRect.MinX = m + IndexToOffset(instance.Name, selection.Value.MinInc);
                selRect.MaxX = m + IndexToOffset(instance.Name, selection.Value.MaxExc + 1);

                Draw.Quad(selRect.Expand(-Onion.Theme.Padding));
                //textInvertRange = (selection.Value.MinInc, selection.Value.MaxExc);
                textColourInstructions[0] = new TextMeshGenerator.ColourInstruction(0, Colors.White);
                textColourInstructions[1] = new TextMeshGenerator.ColourInstruction(selection.Value.MinInc, fg.Color);
                textColourInstructions[2] = new TextMeshGenerator.ColourInstruction(selection.Value.MaxExc, Colors.White);
                drawSelectionTextColour = true;
            }

            Draw.Colour = col;
            Draw.Text(instance.Name, offset, new Vector2(ratio),
                HorizontalTextAlign.Left, VerticalTextAlign.Middle/*,colours: drawSelectionTextColour ? textColourInstructions : null*/);

            if (instance.HasFocus && cursorBlinkTimer % 1 < 0.3f)
            {
                var rect = new Rect(default, new Vector2(1, instance.Rects.Rendered.Height - Onion.Theme.Padding * 2)).Translate(
                    instance.Rects.ComputedGlobal.MinX + textOffset,
                    (instance.Rects.ComputedGlobal.MinY + instance.Rects.ComputedGlobal.MaxY) / 2
                    );

                Draw.Colour = Colors.White;
                Draw.Quad(rect.Translate(cursorPosition + Onion.Theme.Padding, 0), 0, 0);
            }
        }
    }

    public static int OffsetToIndex(ReadOnlySpan<char> input, float x, out float snappedOffset)
    {
        for (int i = 0; i < input.Length; i++)
        {
            snappedOffset = Draw.CalculateTextWidth(input[..i]);
            if (snappedOffset >= x)
                return Math.Max(0, i);
        }
        snappedOffset = Draw.CalculateTextWidth(input); //entire width plus last character again
        return input.Length;
    }

    public static float IndexToOffset(ReadOnlySpan<char> input, int index)
    {
        if (input.IsEmpty)
            return 0;
        if (index >= input.Length)
            return Draw.CalculateTextWidth(input);
        index = Utilities.Clamp(index, 0, input.Length - 1);
        return Draw.CalculateTextWidth(input[..index]);
    }

    public void OnEnd(in ControlParams p)
    {
    }
}
