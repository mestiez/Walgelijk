using System;
using System.Collections.Generic;
using System.Linq;

namespace Walgelijk.SimpleDrawing
{
    internal static class ArrayExtensions
    {
        public static bool SequenceEqualsNullable<T>(this T[]? a, T[]? b)
        {
            if (a == null && b == null)
                return true;
            if (a == null && b != null)
                return false;
            if (a != null && b == null)
                return false;

            return a!.SequenceEqual(b!);
        }
    }

    public struct CachableTextDrawing : IEquatable<CachableTextDrawing>
    {
        public string Text;
        public Color Color;
        public Font? Font;
        public float TextBoxWidth;
        public VerticalTextAlign VerticalAlign;
        public HorizontalTextAlign HorizontalAlign;
        public TextMeshGenerator.ColourInstruction[]? ColourInstructions;

        public override bool Equals(object? obj)
        {
            return obj is CachableTextDrawing drawing && Equals(drawing);

        }

        public bool Equals(CachableTextDrawing drawing)
        {
            return Text == drawing.Text &&
                    EqualityComparer<Color>.Default.Equals(Color, drawing.Color) &&
                    EqualityComparer<Font>.Default.Equals(Font, drawing.Font) &&
                    ColourInstructions.SequenceEqualsNullable(drawing.ColourInstructions) &&
                    TextBoxWidth == drawing.TextBoxWidth &&
                    VerticalAlign == drawing.VerticalAlign &&
                    HorizontalAlign == drawing.HorizontalAlign;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text, Color, Font, TextBoxWidth, VerticalAlign, HorizontalAlign, ColourInstructions);
        }

        public static bool operator ==(CachableTextDrawing left, CachableTextDrawing right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CachableTextDrawing left, CachableTextDrawing right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// A drawing instruction for text
    /// </summary>
    public struct TextDrawing : IEquatable<TextDrawing>
    {
        /// <summary>
        /// The text
        /// </summary>
        public string? Text;
        /// <summary>
        /// The font. Will fall back to default font if null.
        /// </summary>
        public Font? Font;
        /// <summary>
        /// The width before wrapping
        /// </summary>
        public float TextBoxWidth;
        /// <summary>
        /// Vertical alignment
        /// </summary>
        public VerticalTextAlign VerticalAlign;
        /// <summary>
        /// Horizontal alignment
        /// </summary>
        public HorizontalTextAlign HorizontalAlign;

        /// <summary>
        /// Value from 0.0 to 1.0 that determines the percentage of the text to actually draw. This is usually used for "writing" animations for things like dialogue.
        /// </summary>
        public float TextDrawRatio;

        /// <summary>
        /// The <see cref="ColourInstructions"/> that are used for the mesh generation
        /// </summary>
        public TextMeshGenerator.ColourInstruction[]? ColourInstructions;

        public override bool Equals(object? obj)
        {
            return obj is TextDrawing drawing && Equals(drawing);
        }

        public bool Equals(TextDrawing drawing)
        {
            return Text == drawing.Text &&
                   EqualityComparer<Font?>.Default.Equals(Font, drawing.Font) &&
                   TextBoxWidth == drawing.TextBoxWidth &&
                    ColourInstructions.SequenceEqualsNullable(drawing.ColourInstructions) &&
                   VerticalAlign == drawing.VerticalAlign &&
                   HorizontalAlign == drawing.HorizontalAlign;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text, Font, TextBoxWidth, VerticalAlign, HorizontalAlign, ColourInstructions);
        }

        public static bool operator ==(TextDrawing left, TextDrawing right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextDrawing left, TextDrawing right)
        {
            return !(left == right);
        }
    }
}