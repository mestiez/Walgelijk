using System;
using System.Linq;
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

        /// <summary>
        /// Generate 2D text mesh. Returns the local bounding box.
        /// </summary>
        /// <param name="displayString">Text to render</param>
        /// <param name="vertices">Vertex array that will be populated. This need to be the length of displayString * 4</param>
        public Rect Generate(string displayString, Vertex[] vertices)
        {
            float cursor = 0;
            float width = Font.Width;
            float height = Font.Height;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            uint vertexIndex = 0;
            char lastChar = default;
            int line = 0;
            for (int i = 0; i < displayString.Length; i++)
            {
                var c = displayString[i];

                switch (c)
                {
                    case '\n':
                        line++;
                        cursor = 0;
                        continue;
                    //TODO andere escape character handlers
                }

                var glyph = Font.GetGlyph(c);
                Kerning kerning = i == 0 ? default : Font.GetKerning(lastChar, c);

                var pos = new Vector3(cursor + glyph.XOffset + kerning.Amount * KerningMultiplier, -glyph.YOffset - (line * Font.LineHeight * LineHeightMultiplier), 0);

                GlyphUVInfo uvInfo = new GlyphUVInfo(glyph.X / width, glyph.Y / height, glyph.Width / width, glyph.Height / height);

                vertices[vertexIndex + 0] = appendVertex(pos, glyph, uvInfo, Color, 0, 0);
                vertices[vertexIndex + 1] = appendVertex(pos, glyph, uvInfo, Color, 1, 0);
                vertices[vertexIndex + 2] = appendVertex(pos, glyph, uvInfo, Color, 1, 1);
                vertices[vertexIndex + 3] = appendVertex(pos, glyph, uvInfo, Color, 0, 1);

                vertexIndex += 4;
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

            Vertex appendVertex(Vector3 pos, Glyph glyph, GlyphUVInfo uvInfo, Color color, float xFactor, float yFactor)
            {
                var vertex = new Vertex(
                    pos + new Vector3(glyph.Width * xFactor, -glyph.Height * yFactor, 0),
                    new Vector2(uvInfo.X + uvInfo.Width * xFactor, uvInfo.Y + uvInfo.Height * yFactor),
                    color
                    );

                //TODO waarom kan dit niet?? er gaat iets mis maar ik snap niet waarom
                //maxX = MathF.Max(vertex.Position.X, maxX);
                //maxY = MathF.Max(vertex.Position.Y, maxY);
                //minX = MathF.Min(vertex.Position.X, minX);
                //minY = MathF.Min(vertex.Position.Y, minY);

                return vertex;
            }
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
