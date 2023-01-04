using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct Image
    {
        public static void BasicImage(Vector2 topLeft, Vector2 size, IReadableTexture texture, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(nameof(BasicImage).GetHashCode(), site, optionalId, texture.GetHashCode()));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
            PositioningUtils.DisableRaycastRect(id);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            PositioningUtils.ApplyRaycastRect(id, id.DrawBounds.ToRect(), false);

            Gui.PrepareDrawer();
            Draw.Texture = texture;
            Draw.Colour = Color.White;
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Inactive);
            Draw.OutlineColour = Gui.GetOutlineColour(style, State.Inactive);
            Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, State.Inactive));

            Gui.Context.EndControl();
        }

        public static void ImageBox(Vector2 topLeft, Vector2 size, IReadableTexture? texture, IReadableTexture? background, ImageContainmentMode containmentMode, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            Identity id = Gui.Context.StartControl(IdGen.Hash(nameof(ImageBox).GetHashCode(), texture?.GetHashCode() ?? -1, site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
            PositioningUtils.DisableRaycastRect(id);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            Gui.PrepareDrawer();
            var padding = Gui.GetPadding(style, State.Inactive);

            var rect = new Rect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue)
                .StretchToContain(topLeft)
                .StretchToContain(topLeft + size);

            Draw.Colour = Colors.White;
            //Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, State.Inactive));

            if (background != null)
                Gui.Image(topLeft, size, background, style, site, id.Raw);

            if (texture != null)
            {
                Draw.Colour = Colors.White;
               // padding *= 5;
                Draw.Image(texture, new Rect(rect.MinX + padding, rect.MinY + padding, rect.MaxX - padding, rect.MaxY - padding), containmentMode);
            }

            Gui.Context.EndControl();
        }

        public static bool ImageButton(Vector2 topLeft, Vector2 size, IReadableTexture? texture, IReadableTexture? background, ImageContainmentMode containmentMode, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            Identity id = Gui.Context.StartControl(IdGen.Hash(nameof(ImageButton).GetHashCode(), texture?.GetHashCode() ?? -1, site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            State state = Gui.GetStateFor(id);
            var padding = Gui.GetPadding(style, state);

            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            Gui.ProcessButtonLike(id, rect, out var result, out var wasPressed, out var wasReleased, false);
            Gui.PrepareDrawer();
            Gui.ResetAnchor();
            ImageBox(topLeft, size, texture, background, containmentMode, style, site, optionalId + 1);
            id.Cursor = ImguiUtils.GetCursorForState(state);

            if (state == State.Hover)
            {
                Draw.ResetTexture();
                Draw.Colour = Colors.Transparent;
                Draw.OutlineColour = Colors.Red;
                Draw.OutlineWidth = 3;
                Draw.Quad(topLeft, size);
            }

            Gui.Context.EndControl();
            return wasReleased;
        }
    }
}
