using System;
using System.Collections.Generic;

namespace Walgelijk.Imgui
{
    /// <summary>
    /// Represents a UI theme
    /// </summary>
    public struct Style
    {
        /// <summary>
        /// The furtherst background colour. Drawn by layouts. Usually darker than <see cref="Background"/>.
        /// </summary>
        public Color? FarBackground;

        /// <summary>
        /// The background colour. Drawn by most controls.
        /// </summary>
        public StyleProperty<Color>? Background;

        /// <summary>
        /// The text colour
        /// </summary>
        public StyleProperty<Color>? Text;

        /// <summary>
        /// The acccent colour. Drawn by parts of the UI that should be highly visible.
        /// </summary>
        public StyleProperty<Color>? Foreground;

        //public StyleProperty<Color>? Scrollbars;

        /// <summary>
        /// The font.
        /// </summary>
        public Font? Font;

        /// <summary>
        /// The font size in pixels
        /// </summary>
        public StyleProperty<float>? FontSize;

        /// <summary>
        /// The corner roundness
        /// </summary>
        public StyleProperty<float>? Roundness;

        /// <summary>
        /// Padding used throughout the UI
        /// </summary>
        public StyleProperty<float>? Padding;

        /// <summary>
        /// The outline width
        /// </summary>
        public StyleProperty<float>? OutlineWidth;

        /// <summary>
        /// The outline colour
        /// </summary>
        public StyleProperty<Color>? OutlineColour;

        public override bool Equals(object? obj)
        {
            return obj is Style style &&
                   EqualityComparer<Color?>.Default.Equals(FarBackground, style.FarBackground) &&
                   EqualityComparer<StyleProperty<Color>?>.Default.Equals(Background, style.Background) &&
                   EqualityComparer<StyleProperty<Color>?>.Default.Equals(Text, style.Text) &&
                   EqualityComparer<StyleProperty<Color>?>.Default.Equals(Foreground, style.Foreground) &&
                   EqualityComparer<Font?>.Default.Equals(Font, style.Font) &&
                   EqualityComparer<StyleProperty<float>?>.Default.Equals(FontSize, style.FontSize) &&
                   EqualityComparer<StyleProperty<float>?>.Default.Equals(Roundness, style.Roundness) &&
                   EqualityComparer<StyleProperty<float>?>.Default.Equals(Padding, style.Padding) &&
                   EqualityComparer<StyleProperty<float>?>.Default.Equals(OutlineWidth, style.OutlineWidth) &&
                   EqualityComparer<StyleProperty<Color>?>.Default.Equals(OutlineColour, style.OutlineColour);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(FarBackground);
            hash.Add(Background);
            hash.Add(Text);
            hash.Add(Foreground);
            hash.Add(Font);
            hash.Add(FontSize);
            hash.Add(Roundness);
            hash.Add(Padding);
            hash.Add(OutlineWidth);
            hash.Add(OutlineColour);
            return hash.ToHashCode();
        }

        public static bool operator ==(Style left, Style right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Style left, Style right)
        {
            return !(left == right);
        }
    }
}