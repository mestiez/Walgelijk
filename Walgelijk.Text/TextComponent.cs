using System.Linq;
using System.Numerics;

namespace Walgelijk.Text
{
    public class TextComponent : IShapeComponent
    {
        private string displayString;
        private Font font;
        private Color color = Color.White;
        private float tracking = .7f;
        private float lineHeightMultiplier = .7f;

        public TextComponent(string displayString, Font font)
        {
            this.displayString = displayString;
            this.font = font;

            VertexBuffer = new VertexBuffer();
            VertexBuffer.PrimitiveType = Primitive.Quads;
            RenderTask = new ShapeRenderTask(VertexBuffer, Matrix4x4.Identity, font.Material);

            CreateVertices();
        }

        /// <summary>
        /// Displayed string. Changing this forces a vertex array update.
        /// </summary>
        public string String { get => displayString; set { displayString = value; CreateVertices(); } }
        /// <summary>
        /// Used font. Changing this forces a vertex array update.
        /// </summary>
        public Font Font { get => font; set { font = value; CreateVertices(); } }
        /// <summary>
        /// Text colour. Changing this forces a vertex array update.
        /// </summary>
        public Color Color { get => color; set { color = value; CreateVertices(); } }

        public VertexBuffer VertexBuffer { get; private set; }

        public ShapeRenderTask RenderTask { get; private set; }

        /// <summary>
        /// Distance between letters. Changing this forces a vertex array update.
        /// </summary>
        public float Tracking { get => tracking; set { tracking = value; CreateVertices(); } }

        /// <summary>
        /// Distance between each line.  Changing this forces a vertex array update.
        /// </summary>
        public float LineHeightMultiplier { get => lineHeightMultiplier; set { lineHeightMultiplier = value; CreateVertices(); } }

        private void CreateVertices()
        {
            VertexBuffer.Vertices = new Vertex[displayString.Length * 4];

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

                var glyph = Font.GetGlyph(c);

                Kerning kerning = i == 0 ? default : font.Kernings.FirstOrDefault(k => k.FirstChar == lastChar && k.SecondChar == c);

                var pos = new Vector3(cursor + glyph.XOffset + kerning.Amount, glyph.YOffset + line * Font.LineHeight * LineHeightMultiplier, 0);

                float x = glyph.X / width;
                float y = glyph.Y / height;
                float w = glyph.Width / width;
                float h = glyph.Height / height;

                // bottom left
                VertexBuffer.Vertices[vertexIndex] = new Vertex(
                    pos + new Vector3(0, 0, 0),
                    new Vector2(x, y),
                    Color
                    );

                // bottom right
                VertexBuffer.Vertices[vertexIndex + 1] = new Vertex(
                    pos + new Vector3(glyph.Width, 0, 0),
                    new Vector2(x + w, y),
                    Color
                    );

                // top right
                VertexBuffer.Vertices[vertexIndex + 2] = new Vertex(
                    pos + new Vector3(glyph.Width, glyph.Height, 0),
                    new Vector2(x + w, y + h),
                    Color
                    );

                // top left
                VertexBuffer.Vertices[vertexIndex + 3] = new Vertex(
                    pos + new Vector3(0, glyph.Height, 0),
                    new Vector2(x, y + h),
                    Color
                    );

                vertexIndex += 4;
                cursor += (glyph.Advance + glyph.XOffset + kerning.Amount) * tracking;

                lastChar = c;
            }

            VertexBuffer.GenerateIndices();
            VertexBuffer.HasChanged = true;
        }
    }
}
