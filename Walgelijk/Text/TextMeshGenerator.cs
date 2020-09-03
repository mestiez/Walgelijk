using System;
using System.Linq;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Utility class that provides text mesh generation functions 
    /// </summary>
    public static class TextMeshGenerator
    {
        //TODO dit kan beter een instance class zijn met opties en een super simpele Generate() methode

        /// <summary>
        /// Generate 2D text mesh. Returns the local bounding box.
        /// </summary>
        /// <param name="displayString">Text to render</param>
        /// <param name="font">Font to render with</param>
        /// <param name="vertices">Vertex array that will be populated. This need to be the length of displayString * 4</param>
        /// <param name="color">Color to set the vertices with</param>
        /// <param name="kerningAmount">Kerning multiplier</param>
        /// <param name="lineHeightMultiplier">Line height multiplier</param>
        /// <param name="trackingMultiplier">Tracking multiplier</param>
        public static Rect GenerateVertices(string displayString, Font font, Vertex[] vertices, Color color, float trackingMultiplier = 1, float kerningAmount = 1, float lineHeightMultiplier = 1)
        {
            float cursor = 0;
            float width = font.Width;
            float height = font.Height;

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
                }

                var glyph = font.GetGlyph(c);
                Kerning kerning = i == 0 ? default : font.GetKerning(lastChar, c);

                var pos = new Vector3(cursor + glyph.XOffset + kerning.Amount * kerningAmount, -glyph.YOffset - (line * font.LineHeight * lineHeightMultiplier), 0);

                GlyphUVInfo uvInfo = new GlyphUVInfo(glyph.X / width, glyph.Y / height, glyph.Width / width, glyph.Height / height);

                vertices[vertexIndex + 0] = AppendVertex(pos, glyph, uvInfo, color, 0, 0);
                vertices[vertexIndex + 1] = AppendVertex(pos, glyph, uvInfo, color, 1, 0);
                vertices[vertexIndex + 2] = AppendVertex(pos, glyph, uvInfo, color, 1, 1);
                vertices[vertexIndex + 3] = AppendVertex(pos, glyph, uvInfo, color, 0, 1);

                vertexIndex += 4;
                cursor += (glyph.Advance + (pos.X - cursor)) * trackingMultiplier;

                lastChar = c;
            }

            Rect bounding;

            //TODO dit is niet goed
            bounding.MinX = vertices.Min(v => v.Position.X);
            bounding.MinY = vertices.Min(v => v.Position.Y);
            bounding.MaxX = vertices.Max(v => v.Position.X);
            bounding.MaxY = vertices.Max(v => v.Position.Y);

            return bounding;
        }

        private static Vertex AppendVertex(Vector3 pos, Glyph glyph, GlyphUVInfo uvInfo, Color color, float xFactor, float yFactor)
        {
            var vertex = new Vertex(
                pos + new Vector3(glyph.Width * xFactor, -glyph.Height * yFactor, 0),
                new Vector2(uvInfo.X + uvInfo.Width * xFactor, uvInfo.Y + uvInfo.Height * yFactor),
                color
                );
            return vertex;
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
