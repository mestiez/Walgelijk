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
        private IList<ColourInstruction>? colourInstructions = null;

        /// <summary>
        /// The raw text mesh generator. You can edit this all you want but it's safer to use the properties of the <see cref="TextComponent"/>
        /// </summary>
        public readonly TextMeshGenerator TextMeshGenerator;

        /// <summary>
        /// The most recently created text mesh generation result.
        /// </summary>
        public TextMeshResult LastGenerationResult;

        /// <summary>
        /// Create a text component
        /// </summary>
        public TextComponent(string? displayString = null, Font? font = null)
        {
            this.displayString = displayString ?? "";
            this.font = font ?? Font.Default;

            VertexBuffer = new VertexBuffer();
            VertexBuffer.PrimitiveType = Primitive.Triangles;
            RenderTask = new ShapeRenderTask(VertexBuffer, Matrix4x4.Identity, this.font.Material);

            TextMeshGenerator = new TextMeshGenerator
            {
                ParseRichText = true,
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
                TextMeshGenerator.Font = value;
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
                TextMeshGenerator.Color = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// Text colour instructions
        /// </summary>
        public IList<ColourInstruction>? ColorInstructions
        {
            get => colourInstructions;
            set => colourInstructions = value;
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
                TextMeshGenerator.TrackingMultiplier = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// Should the generator parse rich text? Changing this forces a vertex array update.
        /// </summary>
        public bool ParseRichText
        {
            get => TextMeshGenerator.ParseRichText;
            set
            {
                TextMeshGenerator.ParseRichText = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// The maximum text width before wrapping. Changing this forces a vertex array update.
        /// </summary>
        public float WrappingWidth
        {
            get => TextMeshGenerator.WrappingWidth;
            set
            {
                TextMeshGenerator.WrappingWidth = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// The vertical text alignment. Changing this forces a vertex array update.
        /// </summary>
        public VerticalTextAlign VerticalAlignment
        {
            get => TextMeshGenerator.VerticalAlign;
            set
            {
                TextMeshGenerator.VerticalAlign = value;
                CreateVertices();
            }
        }

        /// <summary>
        /// The horizontal text alignment. Changing this forces a vertex array update.
        /// </summary>
        public HorizontalTextAlign HorizontalAlignment
        {
            get => TextMeshGenerator.HorizontalAlign;
            set
            {
                TextMeshGenerator.HorizontalAlign = value;
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
                TextMeshGenerator.KerningMultiplier = value;
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
                TextMeshGenerator.LineHeightMultiplier = value;
                CreateVertices();
            }
        }

        private void CreateVertices()
        {
            if (VertexBuffer.Vertices.Length < displayString.Length * 4)
            {
                VertexBuffer.Vertices = new Vertex[displayString.Length * 4];
                VertexBuffer.Indices = new uint[displayString.Length * 6];
            }

            LastGenerationResult = TextMeshGenerator.Generate(String, VertexBuffer.Vertices, VertexBuffer.Indices, colourInstructions);
            LocalBoundingBox = LastGenerationResult.LocalBounds;
            VertexBuffer.AmountOfIndicesToRender = LastGenerationResult.IndexCount;
            VertexBuffer.HasChanged = true;
        }
    }
}
