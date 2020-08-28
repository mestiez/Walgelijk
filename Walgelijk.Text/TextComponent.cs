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
        private float kerningAmount = 1f;

        public TextComponent(string displayString, Font font = null)
        {
            this.displayString = displayString;
            this.font = font ?? Font.Default;

            VertexBuffer = new VertexBuffer();
            VertexBuffer.PrimitiveType = Primitive.Quads;
            RenderTask = new ShapeRenderTask(VertexBuffer, Matrix4x4.Identity, this.font.Material);

            CreateVertices();
        }

        /// <summary>
        /// Displayed string. Changing this forces a vertex array update.
        /// </summary>
        public string String { get => displayString; set { if (value == displayString) return;  displayString = value; CreateVertices(); } }
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
        public bool ScreenSpace { get; set; }

        /// <summary>
        /// Distance between letters. Changing this forces a vertex array update.
        /// </summary>
        public float TrackingMultiplier { get => tracking; set { tracking = value; CreateVertices(); } }

        /// <summary>
        /// Kerning amount multiplier. Changing this forces a vertex array update.
        /// </summary>
        public float KerningMultiplier { get => kerningAmount; set { kerningAmount = value; CreateVertices(); } }

        /// <summary>
        /// Distance between each line.  Changing this forces a vertex array update.
        /// </summary>
        public float LineHeightMultiplier { get => lineHeightMultiplier; set { lineHeightMultiplier = value; CreateVertices(); } }

        private void CreateVertices()
        {
            VertexBuffer.Vertices = new Vertex[displayString.Length * 4];

            TextMeshGenerator.GenerateVertices(String, Font, VertexBuffer.Vertices, Color, TrackingMultiplier, KerningMultiplier, LineHeightMultiplier);

            VertexBuffer.GenerateIndices();
            VertexBuffer.HasChanged = true;
        }
    }
}
