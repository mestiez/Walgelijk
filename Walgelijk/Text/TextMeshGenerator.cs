using System;
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
        public static Vector4 GenerateVertices(string displayString, Font font, Vertex[] vertices, Color color, float trackingMultiplier = 1, float kerningAmount = 1, float lineHeightMultiplier = 1)
        {
            Vector4 bounding = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

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

                vertices[vertexIndex + 0] = AppendVertex(pos, glyph, uvInfo, color, 0, 0, ref bounding);
                vertices[vertexIndex + 1] = AppendVertex(pos, glyph, uvInfo, color, 1, 0, ref bounding);
                vertices[vertexIndex + 2] = AppendVertex(pos, glyph, uvInfo, color, 1, 1, ref bounding);
                vertices[vertexIndex + 3] = AppendVertex(pos, glyph, uvInfo, color, 0, 1, ref bounding);

                vertexIndex += 4;
                cursor += (glyph.Advance + (pos.X - cursor)) * trackingMultiplier;

                lastChar = c;
            }

            return bounding;
        }

        private static Vertex AppendVertex(Vector3 pos, Glyph glyph, GlyphUVInfo uvInfo, Color color, float xFactor, float yFactor, ref Vector4 bounding)
        {
            var vertex = new Vertex(
                pos + new Vector3(glyph.Width * xFactor, -glyph.Height * yFactor, 0),
                new Vector2(uvInfo.X + uvInfo.Width * xFactor, uvInfo.Y + uvInfo.Height * yFactor),
                color
                );
            RecalculateBoundingBox(vertex, ref bounding);

            return vertex;
        }

        private static void RecalculateBoundingBox(Vertex vert, ref Vector4 bounding)
        {
            bounding.X = MathF.Min(bounding.X, vert.Position.X);
            bounding.Y = MathF.Min(bounding.Y, vert.Position.Y);

            bounding.Z = MathF.Max(bounding.Z, vert.Position.X);
            bounding.W = MathF.Max(bounding.W, vert.Position.Y);
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
            this.X = x;
            this.Y = y;
            this.Width = w;
            this.Height = h;
        }

        public override bool Equals(object obj)
        {
            return obj is GlyphUVInfo other &&
                   X == other.X &&
                   Y == other.Y &&
                   Width == other.Width &&
                   Height == other.Height;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }
    }
}
