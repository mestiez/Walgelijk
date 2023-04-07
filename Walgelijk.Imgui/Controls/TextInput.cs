using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct TextInput
    {
        private static readonly Dictionary<int, (string value, float lastValid)> decimalInputCache = new();

        public static bool DecimalInput(ref float value, Vector2 topLeft, Vector2 size, string? placeholder = null, int maxChar = 9, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            int unique = Walgelijk.Imgui.IdGen.Hash(site, optionalId, nameof(DecimalInput).GetHashCode());

            if (!decimalInputCache.TryGetValue(unique, out var s))
                decimalInputCache.Add(unique, (value.ToString(global::System.Globalization.CultureInfo.InvariantCulture), value));

            if (MathF.Abs(decimalInputCache[unique].lastValid - value) >= float.Epsilon)
                s = decimalInputCache[unique] = (value.ToString(global::System.Globalization.CultureInfo.InvariantCulture), value);

            if (s.value == null)
                s.value = string.Empty;

            if (Gui.TextInput(topLeft, size, ref s.value, TextInputMode.Decimals, false, placeholder, maxChar, style, site, optionalId))
            {
                if (float.TryParse(s.value, global::System.Globalization.NumberStyles.Float, global::System.Globalization.CultureInfo.InvariantCulture, out var v))
                    value = v;

                s.lastValid = value;

                decimalInputCache[unique] = s;
                return true;
            }
            return false;
        }


        public static bool Process(Vector2 topLeft, Vector2 size, ref string value, TextInputMode mode = TextInputMode.All, bool multiline = false, string? placeholder = null, int maxChars = int.MaxValue, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(nameof(TextInput).GetHashCode(), mode.GetHashCode(), placeholder?.GetHashCode() ?? 0, site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);

            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            //rect = PositioningUtils.GetRectIntersection(rect, id.DrawBounds.ToRect());

            PositioningUtils.ApplyRaycastRect(id, rect, true);

            bool result = false;
            Gui.PrepareDrawer();
            Draw.FontSize = Gui.GetFontSize(style, State.Active);
            Draw.Font = Gui.GetFont(style);

            State state = Gui.GetStateFor(id);
            float passwordCharacterLength = Gui.GetFontSize(style, state) * 0.5f;

            if (Gui.ContainsMouse(id, false))
                Gui.Context.Hot = id;
            else if (Gui.Context.IsHot(id))
                Gui.Context.Hot = null;

            if (Gui.Context.IsHot(id))
            {
                if (MathF.Abs(Gui.Input.ScrollDelta) > float.Epsilon)
                    Gui.Context.RequestScrollInput(id);
                if (id.LocalInputState.HasScrollFocus)
                {
                    if (id.LocalInputState.ScrollDelta > 0)
                        id.InnerScrollOffset.X += Draw.FontSize;
                    else
                        id.InnerScrollOffset.X -= Draw.FontSize;
                }

                if (Gui.Input.IsButtonPressed(Walgelijk.MouseButton.Left))
                    Gui.Context.Active = id;
            }

            if (Gui.Context.IsActive(id))
            {
                if (Gui.Input.IsKeyPressed(Key.Left))
                {
                    if (mode != TextInputMode.Password && Gui.Input.IsKeyHeld(Key.LeftControl)) //mag niet aan de user laten zien (of hints geven) wat erin zit als het een password is
                    {
                        for (int i = Gui.Context.TextInputCaretPos - 1; i >= 0; i--)
                            if (value[i] == ' ' || i == 0)
                            {
                                Gui.Context.TextInputCaretPos = i;
                                break;
                            }
                    }
                    else
                        Gui.Context.TextInputCaretPos--;
                }

                if (Gui.Input.IsKeyPressed(Key.Right))
                {
                    if (mode != TextInputMode.Password && Gui.Input.IsKeyHeld(Key.LeftControl)) //mag niet aan de user laten zien (of hints geven) wat erin zit als het een password is
                    {
                        var nextSpaceIndex = Gui.Context.TextInputCaretPos >= value.Length ? -1 : value.IndexOf(' ', Gui.Context.TextInputCaretPos + 1);
                        if (nextSpaceIndex == -1) nextSpaceIndex = value.Length;
                        Gui.Context.TextInputCaretPos = nextSpaceIndex;
                    }
                    else
                        Gui.Context.TextInputCaretPos++;
                }

                if (Gui.Input.IsKeyPressed(Key.End))
                {
                    Gui.Context.TextInputCaretPos = value.Length;
                    id.InnerScrollOffset.X = float.MaxValue;
                }

                if (Gui.Input.IsKeyPressed(Key.Home))
                {
                    Gui.Context.TextInputCaretPos = 0;
                    id.InnerScrollOffset.X = 0;
                }

                Gui.Context.TextInputCaretPos = Utilities.Clamp(Gui.Context.TextInputCaretPos, 0, value.Length);

                if (Gui.Input.IsKeyPressed(Key.Delete))
                {
                    result = true;
                    if (value.Length > 0 && Gui.Context.TextInputCaretPos < value.Length)
                        value = value[..Gui.Context.TextInputCaretPos] + value[(Gui.Context.TextInputCaretPos + 1)..];
                }

                for (int i = 0; i < Gui.Input.TextEntered.Length; i++)
                {
                    var c = Gui.Input.TextEntered[i];

                    if (!multiline && c == '\n')
                        continue;

                    if (!ImguiUtils.CharPassesFilter(c, mode))
                        continue;

                    //een decimal kan maar 1 decimal point hebben :)
                    if (mode == TextInputMode.Decimals && (c == ',' || c == '.') && (value.IndexOf(',') != -1 || value.IndexOf('.') != -1))
                        continue;
                    //een nummer kan maar 1 min symbool hebben en die moet aan het begin
                    if ((mode is TextInputMode.Decimals or TextInputMode.Integers) && c == '-' && value.Length != 0)
                        continue;

                    if (c == '\b')
                    {
                        if (value.Length > 0 && Gui.Context.TextInputCaretPos > 0)
                        {
                            value = value[..(Gui.Context.TextInputCaretPos - 1)] + value[Gui.Context.TextInputCaretPos..];
                            Gui.Context.TextInputCaretPos--;
                        }
                    }
                    else if (ImguiUtils.CountVisibleCharacters(value) < maxChars)
                    {
                        value = value.Insert(Gui.Context.TextInputCaretPos, c.ToString());
                        Gui.Context.TextInputCaretPos++;
                    }

                    result = true;
                }

                if (Gui.Input.IsButtonHeld(Walgelijk.MouseButton.Left))
                {
                    if (Gui.ContainsMouse(id, false))
                    {
                        //if (Input.IsButtonPressed(Walgelijk.Button.Left))
                        {
                            float lengthAtMouse = Gui.Input.WindowMousePos.X - rect.MinX - id.InnerScrollOffset.X - Draw.FontSize / 2;
                            int nearestIndex = -1;
                            float nearestDistance = float.MaxValue;
                            for (int i = 0; i < value.Length + 1; i++)
                            {
                                var l = lengthOfSpan(value.AsSpan()[..i]);
                                var d = MathF.Abs(lengthAtMouse - l);
                                if (d < nearestDistance)
                                {
                                    nearestDistance = d;
                                    nearestIndex = i;
                                }
                            }

                            if (nearestIndex != -1)
                                Gui.Context.TextInputCaretPos = nearestIndex;
                        }
                    }
                    else
                    {
                        Gui.Context.Hot = null;
                        Gui.Context.Active = null;
                    }
                }
            }

            Draw.Colour = Gui.GetBackgroundColour(style, state == State.Active ? State.Inactive : state);
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, state);
            Draw.OutlineColour = Gui.GetOutlineColour(style, state);
            Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, state));
            Draw.Font = Gui.GetFont(style);

            var noText = string.IsNullOrWhiteSpace(value);
            var shouldDrawPlaceholder = noText && !string.IsNullOrWhiteSpace(placeholder);

            var spanLength = lengthOfSpan(value);
            if (spanLength > rect.Width)
                id.InnerScrollOffset.X = Utilities.Clamp(id.InnerScrollOffset.X, -lengthOfSpan(value) + size.X - 5, 5);
            else
                id.InnerScrollOffset.X = 0;

            rect.MaxX = float.PositiveInfinity;
            rect.MinX += id.InnerScrollOffset.X;

            if (shouldDrawPlaceholder)
                Gui.DrawTextInRect(rect, placeholder ?? string.Empty, Gui.GetTextColour(style, state).WithAlpha(0.5f), Gui.GetFontSize(style, state), HorizontalTextAlign.Left, VerticalTextAlign.Middle);
            else if (!noText)
            {
                if (mode == TextInputMode.Password)
                {
                    var leftMiddle = new Vector2(rect.MinX + 10, (rect.MaxY + rect.MinY) * 0.5f);
                    float circleSize = passwordCharacterLength / 3;
                    Draw.Colour = Gui.GetTextColour(style, state);
                    for (int i = 0; i < ImguiUtils.CountVisibleCharacters(value); i++)
                        Draw.Circle(new Vector2(leftMiddle.X + (passwordCharacterLength * i), leftMiddle.Y), new Vector2(circleSize));
                }
                else
                    Gui.DrawTextInRect(rect, value, Gui.GetTextColour(style, state), Gui.GetFontSize(style, state), HorizontalTextAlign.Left, VerticalTextAlign.Middle);
            }

            //draw the caret
            if (Gui.Context.IsActive(id) /*&& Game.Main.Time.SecondsSinceLoadUnscaled % 1 > 0.5f*/)
            {
                var l = lengthOfSpan(value.AsSpan()[..Gui.Context.TextInputCaretPos]);
                Draw.Colour = Gui.GetTextColour(style, state);
                Draw.OutlineWidth = 0;
                Draw.Quad(new Vector2(rect.MinX + l + 6, rect.MinY + 5), new Vector2(1, rect.Height - 10));
            }

            Gui.Context.EndControl();
            return result;

            float lengthOfSpan(ReadOnlySpan<char> text) => (mode == TextInputMode.Password) ?
                    passwordCharacterLength * ImguiUtils.CountVisibleCharacters(text) :
                    Draw.CalculateTextWidth(text);
        }
    }
}
