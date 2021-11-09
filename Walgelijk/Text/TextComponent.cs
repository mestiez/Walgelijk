using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Walgelijk.TextMeshGenerator;

namespace Walgelijk
{
    /// <summary>
    /// IShapeComponent that holds the information necessary to draw text
    /// </summary>
    public class TextComponent : ShapeComponent
    {
        private string displayString;
        private Font font;
        private Color color = Color.White;
        private float trackingMultiplier = 1;
        private float lineHeightMultiplier = 1;
        private float kerningMultiplier = 1f;
        private IList<ColourInstruction> colourInstructions = null;

        private readonly TextMeshGenerator meshGenerator;

        /// <summary>
        /// Create a text component
        /// </summary>
        public TextComponent(string displayString = null, Font font = null)
        {
            this.displayString = displayString ?? "";
            this.font = font ?? Font.Default;

            VertexBuffer = new VertexBuffer();
            VertexBuffer.PrimitiveType = Primitive.Triangles;
            RenderTask = new ShapeRenderTask(VertexBuffer, Matrix4x4.Identity, this.font.Material);

            meshGenerator = new TextMeshGenerator
            {
                Color = Color,
                Font = Font,
                KerningMultiplier = KerningMultiplier,
                LineHeightMultiplier = LineHeightMultiplier,
                TrackingMultiplier = TrackingMultiplier
            };

            CreateVertices();
        }

        /// <summary>
        /// Displayed string. Changing this forces a vertex array update.
        /// </summary>
        public string String
        {
            get => displayString;
            set
            {
                if (value == displayString) 
                    return; 

                displayString = value ?? ""; 
                CreateVertices();
            }
        }

        /// <summary>
        /// Used font. Changing this forces a vertex array update.
        /// </summary>
        public Font Font
        {
            get => font;
            set
            {
                if (value == font)
                    return;

                font = value;
                RenderTask.Material = font.Material;
                meshGenerator.Font = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// Text colour. Changing this forces a vertex array update.
        /// </summary>
        public Color Color
        {
            get => color;
            set
            {
                if (value == color)
                    return;

                color = value;
                meshGenerator.Color = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// Text colour instructions
        /// </summary>
        public IList<ColourInstruction> ColorInstructions
        {
            get => colourInstructions;
            set
            {
                colourInstructions = value;
            }
        }

        /// <summary>
        /// The bounding box of the text in local coordinates
        /// </summary>
        public Rect LocalBoundingBox { get; private set; }

        /// <summary>
        /// Distance between letters. Changing this forces a vertex array update.
        /// </summary>
        public float TrackingMultiplier
        {
            get => trackingMultiplier;
            set
            {
                trackingMultiplier = value; 
                meshGenerator.TrackingMultiplier = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// Kerning amount multiplier. Changing this forces a vertex array update.
        /// </summary>
        public float KerningMultiplier
        {
            get => kerningMultiplier;
            set
            {
                kerningMultiplier = value;
                meshGenerator.KerningMultiplier = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// Distance between each line.  Changing this forces a vertex array update.
        /// </summary>
        public float LineHeightMultiplier
        {
            get => lineHeightMultiplier;
            set
            {
                lineHeightMultiplier = value;
                meshGenerator.LineHeightMultiplier = value;
                CreateVertices();
            }
        }

        private void CreateVertices()
        {
            VertexBuffer.Vertices = new Vertex[displayString.Length * 4];
            VertexBuffer.Indices = new uint[displayString.Length * 6];

            var r = meshGenerator.Generate(String, VertexBuffer.Vertices, VertexBuffer.Indices, colourInstructions);
            LocalBoundingBox = r.LocalBounds;
            VertexBuffer.AmountOfIndicesToRender = r.IndexCount;
            VertexBuffer.HasChanged = true;
        }
    }
}
