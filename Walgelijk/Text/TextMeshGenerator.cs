using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Utility class that provides text mesh generation functions 
    /// </summary>
    public class TextMeshGenerator
    {
        /// <summary>
        /// Font to render with
        /// </summary>
        public Font Font { get; set; } = Font.Default;

        /// <summary>
        /// Color to set the vertices with
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Tracking multiplier
        /// </summary>
        public float TrackingMultiplier { get; set; } = 1;

        /// <summary>
        /// Kerning multiplier
        /// </summary>
        public float KerningMultiplier { get; set; } = 1;

        /// <summary>
        /// Line height multiplier
        /// </summary>
        public float LineHeightMultiplier { get; set; } = 1;

        ///// <summary>
        ///// How to vertically align the text
        ///// </summary>
        //public TextAlign Align { get; set; } = TextAlign.Left;
        //TODO align stuff

        /// <summary>
        /// Parse rich text tags
        /// </summary>
        public bool ParseRichText { get; set; } = false;

        /// <summary>
        /// Generate 2D text mesh. Returns the local bounding box.
        /// </summary>
        /// <param name="displayString">Text to render</param>
        /// <param name="vertices">Vertex array that will be populated. This needs to be the length of displayString * 4</param>
        /// <param name="indices">Index array that will be populated. This needs to be the length of displayString * 6</param>
        /// <param name="colours">Colours to set at indices</param>
        public Rect Generate(string displayString, Vertex[] vertices, uint[] indices, IList<ColourInstruction> colours = null)
        {
            if (vertices.Length != displayString.Length * 4)
                throw new Exception(string.Format("The vertex array is of length {0}, expected {1}", vertices.Length, displayString.Length * 4));
            if (indices.Length != displayString.Length * 6)
                throw new Exception(string.Format("The index array is of length {0}, expected {1}", indices.Length, displayString.Length * 6));

            float cursor = 0;
            float width = Font.Width;
            float height = Font.Height;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            float skew = 0;

            uint vertexIndex = 0;
            uint indexIndex = 0;
            char lastChar = default;
            int line = 0;

            var colorToSet = Colors.White;

            for (int i = 0; i < displayString.Length; i++)
            {
                var c = displayString[i];

                switch (c)
                {
                    case '<': //Possible tag
                        if (!ParseRichText)
                            break;
                        if (i > 0 && displayString[i - 1] == '\\')//check for escape slash
                            break;
                        int closingArrowDistance = displayString.AsSpan()[i..].IndexOf('>');
                        if (closingArrowDistance == -1)//invalid syntax
                            break;
                        executeTag(displayString.AsSpan()[(i+1)..(closingArrowDistance + i)]);
                        i += closingArrowDistance;
                        continue;
                    case '\n':
                        line++;
                        cursor = 0;
                        lastChar = default;
                        continue;
                    case '\t':
                        float tabSize = Font.Size * 5;
                        cursor = MathF.Ceiling(cursor / tabSize) * tabSize;
                        lastChar = default;
                        continue;
                        //TODO andere escape character handlers
                }

                var glyph = Font.GetGlyph(c);
                Kerning kerning = i == 0 ? default : Font.GetKerning(lastChar, c);
                var pos = new Vector3(cursor + glyph.XOffset + kerning.Amount * KerningMultiplier, -glyph.YOffset - (line * Font.LineHeight * LineHeightMultiplier), 0);
                GlyphUVInfo uvInfo = new(glyph.X / width, glyph.Y / height, glyph.Width / width, glyph.Height / height);

                if (colours != null)
                    foreach (var ce in colours)
                        if (ce.CharIndex == i)
                            colorToSet = ce.Colour * Color;

                vertices[vertexIndex + 0] = appendVertex(pos, glyph, uvInfo, colorToSet, 0, 0);
                vertices[vertexIndex + 1] = appendVertex(pos, glyph, uvInfo, colorToSet, 1, 0);
                vertices[vertexIndex + 2] = appendVertex(pos, glyph, uvInfo, colorToSet, 1, 1, skew);
                vertices[vertexIndex + 3] = appendVertex(pos, glyph, uvInfo, colorToSet, 0, 1, skew);

                indices[indexIndex + 0] = vertexIndex + 0;
                indices[indexIndex + 1] = vertexIndex + 1;
                indices[indexIndex + 2] = vertexIndex + 2;

                indices[indexIndex + 3] = vertexIndex + 0;
                indices[indexIndex + 4] = vertexIndex + 3;
                indices[indexIndex + 5] = vertexIndex + 2;

                vertexIndex += 4;
                indexIndex += 6;
                cursor += (glyph.Advance + (pos.X - cursor)) * TrackingMultiplier;

                lastChar = c;
            }

            //TODO dit is niet goed

            for (int i = 0; i < displayString.Length * 4; i++)
            {
                var pos = vertices[i].Position;

                maxX = MathF.Max(pos.X, maxX);
                maxY = MathF.Max(pos.Y, maxY);
                minX = MathF.Min(pos.X, minX);
                minY = MathF.Min(pos.Y, minY);
            }

            return new Rect(minX, minY, maxX, maxY); ;

            Vertex appendVertex(Vector3 pos, Glyph glyph, GlyphUVInfo uvInfo, Color color, float xFactor, float yFactor, float skew = 0)
            {
                var vertex = new Vertex(
                    pos + new Vector3(glyph.Width * xFactor + skew, -glyph.Height * yFactor, 0),
                    new Vector2(uvInfo.X + uvInfo.Width * xFactor + skew, uvInfo.Y + uvInfo.Height * yFactor),
                    color
                    );

                //TODO waarom kan dit niet?? er gaat iets mis maar ik snap niet waarom
                //maxX = MathF.Max(vertex.Position.X, maxX);
                //maxY = MathF.Max(vertex.Position.Y, maxY);
                //minX = MathF.Min(vertex.Position.X, minX);
                //minY = MathF.Min(vertex.Position.Y, minY);

                return vertex;
            }

            void executeTag(ReadOnlySpan<char> tagContents)
            {
#if DEBUG
                Logger.Debug("Executing tag " + tagContents.ToString());
#endif
                bool isClosingTag = tagContents[0] == '/';
                if (isClosingTag)
                    tagContents = tagContents[1..];

                if (tagContents.StartsWith(RichTextTags.Colour)) //Colour tag
                {
                    colorToSet = isClosingTag ? Colors.White : new Color(tagContents[(RichTextTags.Colour.Length + 1)..]);//+1 to remove the equal sign
                    return;
                }

                if (tagContents.StartsWith(RichTextTags.Italic)) //Italics tag
                {
                    skew = isClosingTag ? 0 : Font.Size * /*SkewIntensity*/ 0.0003f;
                    return;
                }
            }
        }

        /// <summary>
        /// Instruction that tells the generator when to set a colour
        /// </summary>
        public struct ColourInstruction
        {
            /// <summary>
            /// Character index at which to set the colour
            /// </summary>
            public int CharIndex;

            /// <summary>
            /// Colour to set when we reach <see cref="CharIndex"/>
            /// </summary>
            public Color Colour;
        }
    }

    internal struct GlyphUVInfo
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public GlyphUVInfo(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }
    }
}
