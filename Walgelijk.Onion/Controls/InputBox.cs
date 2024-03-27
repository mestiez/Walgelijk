using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct InputBox : IControl
{
    public static readonly char[] WordDelimiters =
    {
        ' ',
        '\t',
        '\n',
        ',',
        '.',
        '/',
        '\\',
        '-',
        '(',
        ')',
    };

    private record TextBoxState(bool IncomingChange, in TextBoxOptions Options);
    private static readonly Dictionary<int, TextBoxState> states = new();
    private static readonly OptionalControlState<float> floatInputState = new();

    private static float textOffset;
    private static int cursorIndex;
    private static float cursorPosition;
    private static float slowTimer = 0;
    private static SelectionRange? selection;
    private static float cursorBlinkTimer;
    private static int selectionInitialIndex;
    // private static readonly TextMeshGenerator.ColourInstruction[] textColourInstructions = new TextMeshGenerator.ColourInstruction[3];

    static InputBox()
    {
        Onion.OnClear.AddListener(states.Clear);
    }

    public static bool Int(ref int value, in MinMax<int>? range = null, string? placeholder = null, int identity = 0, [CallerLineNumber] int site = 0)
    {
        float cast = value;
        if (Float(ref cast, range.HasValue ? new MinMax<float>(range.Value.Min, range.Value.Max) : null, placeholder, identity, site))
        {
            value = (int)cast;
            return true;
        }
        return false;
    }

    public static bool Float(ref float value, in MinMax<float>? range = null, string? placeholder = null, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var options = new TextBoxOptions(placeholder, null, null, false, true);

        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(InputBox).GetHashCode(), identity, site), new InputBox());
        instance.RenderFocusBox = true;
        instance.Muted = true;
        Onion.Tree.End();

        var buffer = instance.Name;

        var current = floatInputState.GetValue(instance.Identity);
        if (floatInputState.HasIncomingChange(instance.Identity))
            value = current;
        else
        {
            if (instance.HasScroll)
            {
                if (Onion.Input.ScrollDelta.Y > float.Epsilon)
                    value += 1;
                else if (Onion.Input.ScrollDelta.Y < -float.Epsilon)
                    value -= 1;

                if (range.HasValue)
                    value = Utilities.Clamp(value, range.Value.Min, range.Value.Max);
                instance.Name = value.ToString();
                return true;
            }

            if (MathF.Abs(value - current) > float.Epsilon)
                floatInputState.SetValue(instance.Identity, value);

            if (instance.State is ControlState.None)
                buffer = current.ToString(CultureInfo.InvariantCulture);
        }

        floatInputState.UpdateFor(instance.Identity, ref value);

        if (states.TryGetValue(instance.Identity, out var state))
        {
            if (state.IncomingChange)
            {
                buffer = instance.Name;
                states[instance.Identity] = new(false, options);
                if (float.TryParse(buffer, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                {
                    if (range.HasValue)
                        parsed = Utilities.Clamp(parsed, range.Value.Min, range.Value.Max);
                    floatInputState.SetValue(instance.Identity, parsed);
                    value = parsed;
                }
                return true;
            }

            states[instance.Identity] = state with { Options = options };
            instance.Name = buffer;
            return false;
        }

        instance.Name = buffer;
        states.Add(instance.Identity, new TextBoxState(false, options));

        return false;
    }

    public static bool String(ref string text, in TextBoxOptions options, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(InputBox).GetHashCode(), identity, site), new InputBox());
        instance.RenderFocusBox = true;
        instance.Muted = true;
        Onion.Tree.End();

        if (states.TryGetValue(instance.Identity, out var state))
        {
            if (state.IncomingChange)
            {
                text = instance.Name;
                states[instance.Identity] = new(false, options);
                return true;
            }

            states[instance.Identity] = new(state.IncomingChange, options);
            instance.Name = text;
            return false;
        }

        instance.Name = text;
        states.Add(instance.Identity, new TextBoxState(false, options));
        return false;
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
        if (p.Input.CtrlHeld)
            p.Instance.CaptureFlags |= CaptureFlags.Scroll;

        if (p.Instance.IsHover)
            IControl.SetCursor(DefaultCursor.Text);

        if (hadFocus != p.Instance.HasFocus)
        {
            if (hadFocus)
                selection = null;
            textOffset = 0;
            selectionInitialIndex = -1;
        }

        if (p.Input.DoubleClicked && p.Instance.IsHover && !states[p.Identity].Options.Password)
            SelectWordAt(p, cursorIndex);
        else if (p.Instance.IsActive)
        {
            slowTimer += p.GameState.Time.DeltaTimeUnscaled;
            cursorBlinkTimer = 0;

            var local = p.Input.MousePosition - p.Instance.Rects.ComputedGlobal.BottomLeft;
            local.X -= textOffset;
            local.X -= p.Theme.Padding;

            // we dont use MoveCursor here because this is a special case where the mouse input determines the cursor position
            cursorIndex = OffsetToIndex(p, p.Instance.Name, local.X, out cursorPosition);

            if (p.Input.MousePrimaryPressed && !p.Input.ShiftHeld && !p.Input.DoubleClicked)
            {
                selectionInitialIndex = cursorIndex;
                selection = null;
            }

            SetSelection(selectionInitialIndex, cursorIndex);

            if (slowTimer > 0.05f)
            {
                slowTimer = 0;
                if (cursorPosition < -textOffset)
                    textOffset += p.Theme.FontSize[p.Instance.State];
                else if (cursorPosition > -textOffset + p.Instance.Rects.Rendered.Width)
                    textOffset -= p.Theme.FontSize[p.Instance.State];
            }
        }

        if (p.Instance.HasFocus)
        {
            cursorBlinkTimer += p.GameState.Time.DeltaTimeUnscaled;
            p.Instance.CaptureFlags |= CaptureFlags.Key;

            if (p.Instance.HasKeyboard)
                ProcessKeyInput(p);

            if (p.Instance.HasScroll)
            {
                if (!states[p.Identity].Options.ScrollIsValueChange) // this input box has a number in it so scrolling should not move the text
                {
                    var textWidth = GetTextWidth(p, p.Instance.Name);
                    var maxWidth = p.Instance.Rects.ComputedGlobal.Width - p.Theme.Padding * 2;

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
            var nextIndex = cursorIndex + dir;
            if (p.Input.CtrlHeld && !states[p.Identity].Options.Password)
            {
                if (dir > 0)
                {
                    if (cursorIndex + 1 >= p.Instance.Name.Length)
                        nextIndex = p.Instance.Name.Length;
                    else
                    {
                        nextIndex = p.Instance.Name.IndexOf(' ', cursorIndex + 1);
                        if (nextIndex == -1)
                            nextIndex = p.Instance.Name.Length;
                        else
                            nextIndex = Math.Min(p.Instance.Name.Length, nextIndex + 1);
                    }
                }
                else
                {
                    nextIndex = p.Instance.Name.AsSpan()[0..cursorIndex].LastIndexOf(' ');
                    if (nextIndex == -1)
                        nextIndex = 0;
                }
            }

            if (p.Input.ShiftHeld)
            {
                SetSelection(selectionInitialIndex, nextIndex);
                MoveCursor(p, nextIndex);
            }
            else
            {
                if (IsSelectionValid())
                    MoveCursor(p, dir > 0 ? selection.Value.To : selection.Value.From);
                else
                    MoveCursor(p, nextIndex);

                selection = null;
                selectionInitialIndex = cursorIndex;
            }
        }
        else if (p.Input.ShiftHeld && !IsSelectionValid())
            selectionInitialIndex = cursorIndex;

        if (p.Input.CtrlHeld && p.Input.AlphanumericalHeld.Contains(Key.A))
            selection = (0, p.Instance.Name.Length);

        if (p.Input.HomePressed)
        {
            selection = null;
            selectionInitialIndex = cursorIndex;
            MoveCursor(p, 0);
            if (p.Input.ShiftHeld)
                StretchSelectionTo(0);
        }

        if (p.Input.EndPressed)
        {
            selection = null;
            selectionInitialIndex = cursorIndex;
            MoveCursor(p, p.Instance.Name.Length);
            if (p.Input.ShiftHeld)
                StretchSelectionTo(p.Instance.Name.Length);
        }

        if (p.Input.PastePressed)
        {
            var pasted = TextCopy.ClipboardService.GetText();
            if (pasted != null)
            {
                pasted = pasted.ReplaceLineEndings().Replace(Environment.NewLine, string.Empty);

                if (!string.IsNullOrEmpty(pasted))
                    AppendText(p, pasted);
            }
        }

        if (p.Input.CopyPressed && selection.HasValue && !states[p.Identity].Options.Password && IsSelectionValid())
        {
            var s = selection.Value;
            TextCopy.ClipboardService.SetText(p.Instance.Name[s.From..s.To]);
        }

        // process input text
        if (p.Input.TextEntered.Length > 0)
        {
            string textToAdd = string.Empty;
            int backspaceCount = 0;
            int deleteCount = 0;
            //var regex = states[p.Identity].Options.Filter;
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
            for (int i = 0; i < deleteCount; i++)
                Delete(p);
        }
    }

    [MemberNotNullWhen(true, nameof(selection))]
    private static bool IsSelectionValid() => selection.HasValue && selection.Value.From < selection.Value.To;

    [MemberNotNull(nameof(selection))]
    private static void SetSelection(int from, int to)
    {
        selection = (
            Math.Min(to, from),
            Math.Max(to, from)
            );
    }

    [MemberNotNull(nameof(selection))]
    private static void StretchSelectionTo(int target)
    {
        if (!IsSelectionValid())
            SetSelection(selectionInitialIndex, target);

        if (target < selection.Value.From)
            selection = (target, selection.Value.To);
        else if (target > selection.Value.To)
            selection = (selection.Value.From, target);
    }

    [MemberNotNull(nameof(selection))]
    private static void SelectWordAt(in ControlParams p, int index)
    {
        var str = p.Instance.Name.AsSpan();
        var leftSpace = str[..index].LastIndexOf(' ');
        var rightSpace = str[index..].IndexOf(' ');

        if (rightSpace == -1)
            rightSpace = p.Instance.Name.Length;
        else
            rightSpace += index;

        SetSelection(leftSpace + 1, rightSpace);
    }

    private static void AppendText(in ControlParams p, string text)
    {
        DeleteSelection(p);
        var s = states[p.Identity];
        var old = p.Instance.Name;

        p.Instance.Name = p.Instance.Name.Insert(Utilities.Clamp(cursorIndex, 0, p.Instance.Name.Length), text);
        if (!s.Options.Filter?.IsMatch(p.Instance.Name) ?? false)
            p.Instance.Name = old;

        if (s.Options.MaxLength.HasValue && p.Instance.Name.Length >= s.Options.MaxLength)
            p.Instance.Name = p.Instance.Name[0..s.Options.MaxLength.Value];

        MarkIncomingChange(p);
        MoveCursor(p, cursorIndex + (p.Instance.Name.Length - old.Length));
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
        MarkIncomingChange(p);
    }

    private static bool DeleteSelection(in ControlParams p)
    {
        if (!IsSelectionValid())
            return false;

        selection = (Math.Max(0, selection.Value.From), Math.Min(p.Instance.Name.Length, selection.Value.To));

        p.Instance.Name = p.Instance.Name.Remove(selection.Value.From, (selection.Value.To - selection.Value.From));
        MarkIncomingChange(p);
        MoveCursor(p, selection.Value.From);
        selection = null;
        return true;
    }

    private static void MoveCursor(in ControlParams p, int targetIndex)
    {
        cursorIndex = targetIndex;
        cursorPosition = IndexToOffset(p, p.Instance.Name, cursorIndex);
        cursorIndex = Utilities.Clamp(cursorIndex, 0, p.Instance.Name.Length);

        if (cursorPosition < -textOffset)
            textOffset = -cursorPosition;
        else if (cursorPosition >= -textOffset + p.Instance.Rects.Rendered.Width - p.Theme.Padding)
            textOffset = -cursorPosition + p.Instance.Rects.Rendered.Width - p.Theme.Padding;
        cursorBlinkTimer = 0;
    }

    private static void MarkIncomingChange(in ControlParams p)
    {
        states[p.Instance.Identity] = new TextBoxState(true, states[p.Instance.Identity].Options);
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var t = node.GetAnimationTime();
        var anim = instance.Animations;

        float d = instance.HasFocus ? textOffset : 0;

        var fg = p.Theme.Foreground[instance.State];
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;
        Draw.OutlineColour = p.Theme.OutlineColour[instance.State];
        Draw.OutlineWidth = p.Theme.OutlineWidth[instance.State];

        anim.AnimateColour(ref Draw.OutlineColour, t);
        anim.AnimateRect(ref instance.Rects.Rendered, t);

        if (instance.IsHover)
            Draw.Colour = fg.Color.Brightness(1.2f);

        if (instance.IsActive)
            Draw.Colour = fg.Color.Brightness(0.9f);

        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Quad(instance.Rects.Rendered, 0, p.Theme.Rounding);

        var bg = p.Theme.Background[instance.State];
        Draw.Colour = bg.Color;
        Draw.Texture = bg.Texture;
        Draw.ImageMode = bg.ImageMode;
        anim.AnimateColour(ref Draw.Colour, t);
        Draw.OutlineWidth = 0;
        Draw.Quad(instance.Rects.Rendered.Expand(-2), 0, p.Theme.Rounding);

        Draw.ResetTexture();
        Draw.ImageMode = default;

        Draw.Font = p.Theme.Font;
        Draw.Colour = p.Theme.Text[instance.State];
        anim.AnimateColour(ref Draw.Colour, t);
        if (anim.ShouldRenderText(t))
        {
            if (!instance.HasFocus)
                Draw.Colour.A *= 0.7f;

            var col = Draw.Colour;

            var ratio = instance.Rects.Rendered.Area / instance.Rects.ComputedGlobal.Area;
            var offset = new Vector2(instance.Rects.Rendered.MinX + p.Theme.Padding, 0.5f * (instance.Rects.Rendered.MinY + instance.Rects.Rendered.MaxY));
            offset.X += (int)d;
            if (p.Instance.HasFocus && IsSelectionValid())
            {
                Draw.Colour = (Vector4.One - fg.Color) with { W = col.A * 0.5f };
                var selRect = instance.Rects.Rendered;

                var m = selRect.MinX + textOffset + p.Theme.Padding;
                selRect.MinX = m + IndexToOffset(p, instance.Name, selection.Value.From);
                selRect.MaxX = m + IndexToOffset(p, instance.Name, selection.Value.To);

                selRect.MaxY -= p.Theme.Padding;
                selRect.MinY += p.Theme.Padding;

                Draw.OutlineWidth = 0;
                Draw.Quad(selRect, 0, p.Theme.Rounding);
                //textColourInstructions[0] = new TextMeshGenerator.ColourInstruction(0, Colors.White);
                //textColourInstructions[1] = new TextMeshGenerator.ColourInstruction(selection.Value.From, fg.Color);
                //textColourInstructions[2] = new TextMeshGenerator.ColourInstruction(selection.Value.To, Colors.White);
            }

            if (p.Instance.Name.Length == 0)
            {
                var placeholder = states[node.Identity].Options.Placeholder;
                if (!string.IsNullOrWhiteSpace(placeholder))
                {
                    Draw.Colour = col * 0.5f;
                    Draw.Text(placeholder, offset, new Vector2(ratio), HorizontalTextAlign.Left, VerticalTextAlign.Middle);
                }
            }
            else
            {
                Draw.Colour = col;

                var oldDrawbounds = Draw.DrawBounds;
                Draw.DrawBounds = new DrawBounds(instance.Rects.ComputedDrawBounds.Expand(-p.Theme.Padding));
                if (states[node.Identity].Options.Password)
                {
                    float w = GetPasswordCharWidth(instance);
                    float r = w * 0.25f;
                    for (int i = 0; i < instance.Name.Length; i++)
                        Draw.Circle(offset + new Vector2(i * w + r, 0), new Vector2(r));
                }
                else
                    Draw.Text(instance.Name, offset, new Vector2(ratio), HorizontalTextAlign.Left, VerticalTextAlign.Middle/*,colours: drawSelectionTextColour ? textColourInstructions : null*/);
                Draw.DrawBounds = oldDrawbounds;
            }

            if (instance.HasFocus && cursorBlinkTimer % 1 < 0.5f)
            {
                var rect = new Rect(default, new Vector2(1, instance.Rects.Rendered.Height - p.Theme.Padding * 3)).Translate(
                    instance.Rects.ComputedGlobal.MinX + textOffset,
                    (instance.Rects.ComputedGlobal.MinY + instance.Rects.ComputedGlobal.MaxY) / 2
                    );

                Draw.Colour = p.Theme.Text[ControlState.None];
                Draw.OutlineWidth = 0;
                Draw.Quad(rect.Translate(cursorPosition + p.Theme.Padding, 0), 0, 0);
            }
        }
    }

    public static int OffsetToIndex(in ControlParams p, ReadOnlySpan<char> input, float x, out float snappedOffset)
    {
        for (int i = 0; i < input.Length; i++)
        {
            snappedOffset = GetTextWidth(p, input[..i]);
            if (snappedOffset >= x)
                return Math.Max(0, i);
        }
        snappedOffset = GetTextWidth(p, input);
        return input.Length;
    }

    public static float IndexToOffset(in ControlParams p, ReadOnlySpan<char> input, int index)
    {
        if (input.IsEmpty)
            return 0;

        if (index >= input.Length)
            return GetTextWidth(p, input);

        index = Utilities.Clamp(index, 0, input.Length);
        return GetTextWidth(p, input[..index]);
    }

    private static float GetTextWidth(in ControlParams p, in ReadOnlySpan<char> str)
    {
        var state = states[p.Identity];
        if (state.Options.Password)
            return str.Length * GetPasswordCharWidth(p.Instance);
        Draw.Font = p.Theme.Font;
        Draw.FontSize = p.Theme.FontSize[p.Instance.State];
        return Draw.CalculateTextWidth(str);
    }

    public static float GetPasswordCharWidth(ControlInstance p) => p.Theme.FontSize[p.State] * 0.7f;

    public void OnEnd(in ControlParams p)
    {
    }
}

public record struct SelectionRange(int From, int To)
{
    public static implicit operator (int, int)(SelectionRange value) => (value.From, value.To);

    public static implicit operator SelectionRange((int, int) value) => new SelectionRange(value.Item1, value.Item2);
}