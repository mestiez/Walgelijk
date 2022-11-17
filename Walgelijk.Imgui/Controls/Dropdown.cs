using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct Dropdown
    {
        public static int? ActiveDropdown = null;

        private static EnumNameCache enumValueCache = new();

        public static bool ProcessEnumDropdown<T>(Vector2 topLeft, Vector2 size, ref T selected, int maxItemsBeforeScrolling = 5, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0) where T : notnull, global::System.Enum
        {
            var (names, values) = enumValueCache.Load(typeof(T));
            int index = Array.IndexOf(names, Enum.GetName(typeof(T), selected));
            if (Process(topLeft, size, names, ref index, maxItemsBeforeScrolling, style, site, optionalId))
            {
                var v = values.GetValue(index);
                if (v != null)
                    selected = (T)v;
                return true;
            }
            return false;
        }

        public static bool Process(Vector2 topLeft, Vector2 size, IEnumerable<string>? options, ref int selectedIndex, int maxItemsBeforeScrolling = 5, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var selectedText = options == null ? string.Empty : options.ElementAt(selectedIndex);

            Identity id = Gui.Context.StartControl(IdGen.Hash(nameof(Dropdown).GetHashCode(), site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            PositioningUtils.ApplyRaycastRect(id, rect, true);

            State state = Gui.GetStateFor(id);

            id.Cursor = ImguiUtils.GetCursorForState(state);

            Gui.PrepareDrawer();
            Draw.Colour = Gui.GetBackgroundColour(style, state);
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, state);
            Draw.OutlineColour = Gui.GetOutlineColour(style, state);
            Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, state));
            Draw.Font = Gui.GetFont(style);
            Gui.DrawTextInRect(rect, selectedText, Gui.GetTextColour(style, state), Gui.GetFontSize(style, state), HorizontalTextAlign.Left, VerticalTextAlign.Middle);
            //TODO de hele dropdown

            const float triangleSize = 0.4f;
            bool shouldRenderUpsideDown = ActiveDropdown != id.Raw;
            Draw.OutlineWidth = 0;
            if (shouldRenderUpsideDown)
                Draw.TriangleIsco(topLeft + new Vector2(size.X - size.Y * 0.125f - 5, size.Y - size.Y / 4), new Vector2(size.Y * triangleSize, size.Y * triangleSize), 180);
            else
                Draw.TriangleIsco(topLeft + new Vector2(size.X - size.Y * 0.5f - 5, (size.Y * (1 - triangleSize)) - 1 - size.Y / 4), new Vector2(size.Y * triangleSize, size.Y * triangleSize), 0);

            bool interacted = false;
            bool isMouseInsideDropdown = false;
            if (options != null && options.Any() && ActiveDropdown == id.Raw)
            {
                float listHeight = Math.Min(maxItemsBeforeScrolling, options.Count()) * (size.Y + Gui.GetPadding(style, State.Inactive)) + Gui.GetPadding(style, State.Inactive);
                var oldOrder = Draw.Order;
                Gui.EscapeLayout();
                Gui.Context.Order = Draw.Order = RenderOrder.UI.WithOrder(int.MaxValue);
                Gui.StartVerticalLayout(rect.BottomLeft + new Vector2(0, size.Y), new Vector2(size.X, listHeight), ArrayLayoutMode.Start, ArrayScaleMode.Stretch, style);
                var layout = Gui.Context.LastCreatedIdentity ?? throw new Exception("Vertical dropdown layout could not be created");
                layout.Order = int.MaxValue - 1;
                var layoutRect = new Rect(layout.TopLeft.X, layout.TopLeft.Y, layout.TopLeft.X + layout.Size.X, layout.TopLeft.Y + layout.Size.Y);
                isMouseInsideDropdown = layoutRect.ContainsPoint(Gui.Input.WindowMousePos);
                if (!Gui.Context.IsAnythingHot() && isMouseInsideDropdown)
                    Gui.Context.Hot = id;
                if (!isMouseInsideDropdown && Gui.Context.IsHot(id))
                    Gui.Context.Hot = null;
                int i = 0;
                foreach (var item in options)
                {
                    if (Gui.Button(item, new Vector2(0, i), size, out var _, out var _, HorizontalTextAlign.Left, VerticalTextAlign.Middle, style, optionalId: id.Raw))
                    {
                        interacted = true;
                        ActiveDropdown = null;
                        selectedIndex = i;
                    }
                    i++;
                }
                Gui.StopVerticalLayout();
                Gui.Context.Order = Draw.Order = oldOrder;
            }

            bool result = false, wasPressed = false, wasReleased = false;
            if (ActiveDropdown != id.Raw)
                Gui.ProcessButtonLike(id, rect, out result, out wasPressed, out wasReleased, true);

            if (wasPressed)
            {
                if (ActiveDropdown == id.Raw)
                    ActiveDropdown = null;
                else
                    ActiveDropdown = id.Raw;
            }

            if (!wasPressed && !interacted && Gui.Input.IsButtonPressed(Walgelijk.Button.Left) && ActiveDropdown == id.Raw && !isMouseInsideDropdown)
                ActiveDropdown = null;

            Gui.Context.EndControl();
            return interacted;
        }

        private class EnumNameCache : Cache<Type, (string[], Array)>
        {
            protected override (string[], Array) CreateNew(Type raw)
            {
                if (!raw.IsEnum)
                    throw new ArgumentException("Given type is not an enum.");

                return (raw.GetEnumNames(), raw.GetEnumValues());
            }

            protected override void DisposeOf((string[], Array) loaded) { }
        }

        public static bool Process<T>(Vector2 topLeft, Vector2 size, (string, T)[] options, ref T? selected, int maxItemsBeforeScrolling, Style? style, int site, int optionalId) where T : notnull
        {
            int index = -1;
            for (int i = 0; i < options.Length; i++)
                if (options[i].Item2.Equals(selected))
                {
                    index = i;
                    break;
                }

            if (index == -1)
                index = 0;
            if (Process(topLeft, size, options.Select(static s => s.Item1), ref index, maxItemsBeforeScrolling, style, site, optionalId))
            {
                selected = options[index].Item2;
                return true;
            }
            return false;
        }
    }
}
