using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct Colourpicker
    {
        private static Texture? rainbowTexture = null;
        private static readonly HexadecimalStringCache stringCache = new();
        private static float staticH, staticS, staticV;
        private static float staticR, staticG, staticB;

        private class HexadecimalStringCache : Cache<int, char[]>
        {
            protected override char[] CreateNew(int raw)
            {
                return new char[] { '\n', '0', '0', '0', '0', '0', '0' };
            }

            protected override void DisposeOf(char[] loaded)
            {
                //heeft geen zin lmao
            }
        }

        public static bool Process(Vector2 topLeft, Vector2 size, bool editableAlpha, ref Color color, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            //rainbowTexture.Dispose();
            //rainbowTexture = null;
            if (rainbowTexture == null)
            {
                rainbowTexture = new Texture(1, 256);
                rainbowTexture.RawData = new Color[rainbowTexture.Width * rainbowTexture.Height];
                ColourPickerRendering.FillRainbow(rainbowTexture);
            }

            var hsv = ColourUtils.RGBtoHSV(color);

            var mainID = Gui.Context.StartControl(IdGen.Hash(nameof(Colourpicker).GetHashCode(), site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, mainID, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, mainID, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, mainID, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, mainID, ref topLeft);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, mainID, new DrawBounds(size, topLeft));

            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            PositioningUtils.ApplyRaycastRect(mainID, rect, true);
            bool result = false;

            const float padding = 5;
            const float resultBarHeight = 24;
            const float hueBarWidth = 24;
            float rightBarWidth = size.Y > 90 ? 80 : 0;

            var bg = Gui.GetBackgroundColour(style, State.Active);
            var fg = Gui.GetForegroundColour(style, State.Inactive);
            var txt = Gui.GetTextColour(style, State.Inactive);
            var fontSize = Gui.GetFontSize(style, State.Inactive);
            var roundness = Gui.GetRoundness(style, State.Inactive);
            Draw.Font = Gui.GetFont(style);

            var saturationValueRect = new Rect(
                topLeft.X + padding, topLeft.Y + padding,
                topLeft.X + padding + rect.Width - padding * 3 - rightBarWidth - hueBarWidth - padding,
                topLeft.Y + padding + rect.Height - padding * 3 - resultBarHeight);

            var hueRect = new Rect(
                saturationValueRect.MinX + saturationValueRect.Width + padding, saturationValueRect.MinY,
                saturationValueRect.MinX + saturationValueRect.Width + padding + hueBarWidth, saturationValueRect.MaxY);

            var saturationValueID = Gui.Context.StartControl(IdGen.Hash(mainID.Raw, 10));
            saturationValueID.DrawBounds = mainID.DrawBounds;
            PositioningUtils.ApplyRaycastRect(saturationValueID, saturationValueRect, true);
            Gui.Context.EndControl();

            var hueSliderId = Gui.Context.StartControl(IdGen.Hash(mainID.Raw, 20));
            hueSliderId.DrawBounds = mainID.DrawBounds;
            PositioningUtils.ApplyRaycastRect(hueSliderId, hueRect, true);
            Gui.Context.EndControl();

            Gui.ProcessButtonLike(saturationValueID, saturationValueRect, out var svHeld, out var svPressed, out var svReleased);
            Gui.ProcessButtonLike(hueSliderId, hueRect, out var hHeld, out var hPressed, out var hReleased);

            if (svHeld)
            {
                if (Gui.Input.IsButtonHeld(Walgelijk.Button.Left))
                    hsv.s01 = Utilities.Clamp((Gui.Input.WindowMousePos.X - saturationValueRect.MinX) / saturationValueRect.Width);
                hsv.v01 = Utilities.Clamp(1 - (Gui.Input.WindowMousePos.Y - saturationValueRect.MinY) / saturationValueRect.Height);
                color = ColourUtils.HSVtoRGB(hsv.h01, hsv.s01, hsv.v01);
                result = true;
            }

            if (hHeld)
            {
                hsv.h01 = 1 - Utilities.Clamp((Gui.Input.WindowMousePos.Y - hueRect.MinY) / hueRect.Height);
                color = ColourUtils.HSVtoRGB(hsv.h01, hsv.s01, hsv.v01);
                result = true;
            }

            Gui.PrepareDrawer();

            Draw.Colour = bg;
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Inactive);
            Draw.OutlineColour = Gui.GetOutlineColour(style, State.Inactive);
            Draw.Quad(topLeft, size, roundness: roundness);

            //draw saturation/value thing
            Draw.Colour = Colors.White;
            Draw.ResetTexture();
            Draw.Material = ColourPickerRendering.ColourPickerMaterialCache.Load(Gui.Context.CurrentControlIndex);
            Draw.Material.SetUniform(ColourPickerRendering.Shaders.HueUniform, hsv.h01);
            Draw.OutlineWidth = 0;
            Draw.Quad(saturationValueRect.BottomLeft, saturationValueRect.GetSize(), 0, roundness);
            Draw.ResetMaterial();

            const float circleSize = 5;
            Draw.Colour = ((Color)(Vector4.One - color)).WithAlpha(0.5f);
            var relativeCirclePos = new Vector2(
                Utilities.Clamp(hsv.s01 * saturationValueRect.Width, circleSize, saturationValueRect.Width - circleSize),
                Utilities.Clamp((1 - hsv.v01) * saturationValueRect.Height, circleSize, saturationValueRect.Height - circleSize));

            Draw.Circle(relativeCirclePos + saturationValueRect.BottomLeft,
                new Vector2(circleSize));

            //draw hue thing
            Draw.Colour = Color.White;
            Draw.Texture = rainbowTexture;
            Draw.OutlineWidth = 0;
            Draw.Quad(hueRect.BottomLeft, hueRect.GetSize(), 0, roundness);

            const float huePickerHeight = 5;
            Draw.ResetTexture();
            var relativeHuePickerPosition = new Vector2(0, Utilities.Clamp((1 - hsv.h01) * hueRect.Height, roundness, hueRect.Height - roundness));
            Draw.Colour = Colors.Black.WithAlpha(0.5f);
            Draw.Quad(hueRect.BottomLeft + relativeHuePickerPosition, new Vector2(hueBarWidth, huePickerHeight));
            Draw.Colour = Colors.White;
            Draw.Quad(hueRect.BottomLeft + relativeHuePickerPosition + new Vector2(0, huePickerHeight / 4), new Vector2(hueBarWidth, huePickerHeight / 2));

            Draw.Colour = color;
            Draw.Quad(new Vector2(padding, padding + saturationValueRect.Height + padding) + rect.BottomLeft,
                new Vector2(saturationValueRect.Width + hueBarWidth + padding, resultBarHeight), roundness: roundness);

            //the sliders... verzin iets beters ofzo sukkel
            if (size.Y > 90)
            {
                const float sliderHeight = 24;

                staticR = color.R * 255;
                if (Gui.SliderHorizontal("R:{0}", new Vector2(hueRect.MaxX + padding, hueRect.MinY), new Vector2(rightBarWidth, sliderHeight), ref staticR, 0, 255, 0, 1, style))
                {
                    result = true;
                    color.R = staticR / 255f;
                }

                staticG = color.G * 255;
                if (Gui.SliderHorizontal("G:{0}", new Vector2(hueRect.MaxX + padding, hueRect.MinY + sliderHeight + padding), new Vector2(rightBarWidth, sliderHeight), ref staticG, 0, 255, 0, 1, style))
                {
                    result = true;
                    color.G = staticG / 255f;
                }

                staticB = color.B * 255;
                if (Gui.SliderHorizontal("B:{0}", new Vector2(hueRect.MaxX + padding, hueRect.MinY + ((sliderHeight + padding) * 2)), new Vector2(rightBarWidth, sliderHeight), ref staticB, 0, 255, 0, 1, style))
                {
                    result = true;
                    color.B = staticB / 255f;
                }

                if (size.Y > 200)
                {
                    float cH = (sliderHeight * 4 + padding * 3);
                    hsv = ColourUtils.RGBtoHSV(color);

                    staticH = hsv.h01 * 100;
                    if (Gui.SliderHorizontal("H:{0}%", new Vector2(hueRect.MaxX + padding, hueRect.MinY + cH), new Vector2(rightBarWidth, sliderHeight), ref staticH, 0, 100, 0, 1, style))
                    {
                        result = true;
                        color = ColourUtils.HSVtoRGB(staticH / 100, hsv.s01, hsv.v01);
                    }

                    staticS = hsv.s01 * 100;
                    if (Gui.SliderHorizontal("S:{0}%", new Vector2(hueRect.MaxX + padding, hueRect.MinY + sliderHeight + padding + cH), new Vector2(rightBarWidth, sliderHeight), ref staticS, 0, 100, 0, 1, style))
                    {
                        result = true;
                        color = ColourUtils.HSVtoRGB(hsv.h01, staticS / 100, hsv.v01);
                    }

                    staticV = hsv.v01 * 100;
                    if (Gui.SliderHorizontal("V:{0}%", new Vector2(hueRect.MaxX + padding, hueRect.MinY + ((sliderHeight + padding) * 2) + cH), new Vector2(rightBarWidth, sliderHeight), ref staticV, 0, 100, 0, 1, style))
                    {
                        result = true;
                        color = ColourUtils.HSVtoRGB(hsv.h01, hsv.s01, staticV / 100);
                    }

                    if (size.Y > 250)
                    {
                        cH = hueRect.MinY + ((sliderHeight + padding) * 2) + cH + sliderHeight * 2 + padding;
                        Span<char> hexadecimalString = stringCache.Load(Gui.Context.CurrentControlIndex);
                        if (hexadecimalString[0] == '\n' || result)
                            ColourUtils.ColourToHexadecimal(color, hexadecimalString);

                        var hexString = new string(hexadecimalString);
                        //TODO dit kan NETTER en SNELLER
                        if (Gui.TextInput(new Vector2(hueRect.MaxX + padding, cH), new Vector2(rightBarWidth, sliderHeight), ref hexString, TextInputMode.HexadecimalColourCode, false, "hex code", 7, style))
                        {
                            for (int i = 0; i < hexadecimalString.Length; i++)
                                hexadecimalString[i] = i >= hexString.Length ? '\0' : char.ToUpper(hexString[i]);

                            if (ColourUtils.IsValidHexadecimalColour(hexadecimalString))
                                color = new Color(hexadecimalString);

                            result = true;
                        }
                    }
                }
            }

            Gui.Context.EndControl();
            return result;
        }
    }
}
