using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Utility class that provides text mesh generation functions 
    /// </summary>
    public static class TextMeshGenerator
    {
        /// <summary>
        /// Generate 2D text mesh
        /// </summary>
        /// <param name="displayString">Text to render</param>
        /// <param name="font">Font to render with</param>
        /// <param name="vertices">Vertex array that will be populated. This need to be the length of displayString * 4</param>
        /// <param name="color">Color to set the vertices with</param>
        /// <param name="kerningAmount">Kerning multiplier</param>
        /// <param name="lineHeightMultiplier">Line height multiplier</param>
        /// <param name="trackingMultiplier">Tracking multiplier</param>
        public static void GenerateVertices(string displayString, Font font, Vertex[] vertices, Color color, float trackingMultiplier = 1, float kerningAmount = 1, float lineHeightMultiplier = 1)
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

                var pos = new Vector3(cursor + glyph.XOffset + kerning.Amount * kerningAmount, glyph.YOffset + line * font.LineHeight * lineHeightMultiplier, 0);

                float x = glyph.X / width;
                float y = glyph.Y / height;
                float w = glyph.Width / width;
                float h = glyph.Height / height;

                // bottom left
                vertices[vertexIndex] = new Vertex(
                    pos + new Vector3(0, 0, 0),
                    new Vector2(x, y),
                    color
                    );

                // bottom right
                vertices[vertexIndex + 1] = new Vertex(
                    pos + new Vector3(glyph.Width, 0, 0),
                    new Vector2(x + w, y),
                    color
                    );

                // top right
                vertices[vertexIndex + 2] = new Vertex(
                    pos + new Vector3(glyph.Width, glyph.Height, 0),
                    new Vector2(x + w, y + h),
                    color
                    );

                // top left
                vertices[vertexIndex + 3] = new Vertex(
                    pos + new Vector3(0, glyph.Height, 0),
                    new Vector2(x, y + h),
                    color
                    );

                vertexIndex += 4;
                cursor += (glyph.Advance + (pos.X - cursor)) * trackingMultiplier;

                lastChar = c;
            }
        }
    }
}
