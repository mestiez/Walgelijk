using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Basic rectangle renderer data
    /// </summary>
    public class RectangleShapeComponent : ShapeComponent
    {
        private Vector2 size = Vector2.One;
        private Color color = Color.White;
        private Vector2 pivot = new Vector2(0.5f, 0.5f);

        /// <summary>
        /// Colour of the rectangle
        /// </summary>
        public Color Color
        {
            get => color;
            set
            {
                color = value;
                UpdateVertices();
            }
        }

        /// <summary>
        /// Size of the rectangle
        /// </summary>
        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                UpdateVertices();
            }
        }

        /// <summary>
        /// Material that is drawn with
        /// </summary>
        public Material Material
        {
            get => RenderTask.Material; set
            {
                var rt = RenderTask;
                rt.Material = value;
                RenderTask = rt;
            }
        }

        /// <summary>
        /// Pivot point of the rectangle ranging from (0.0, 0.0) to (1.0, 1.0). Default is (0.5, 0.5).
        /// </summary>
        public Vector2 Pivot
        {
            get => pivot;
            set
            {
                pivot = value;
                UpdateVertices();
            }
        }

        /// <summary>
        /// Create a rectangle renderer component
        /// </summary>
        public RectangleShapeComponent()
        {
            VertexBuffer = new VertexBuffer()
            {
                PrimitiveType = Primitive.Quads,
            };

            RenderTask = new ShapeRenderTask(VertexBuffer)
            {
                ModelMatrix = Matrix4x4.Identity,
                VertexBuffer = VertexBuffer
            };

            UpdateVertices();
        }

        private void UpdateVertices()
        {
            var correctedPivot = pivot;
            correctedPivot.Y = 1 - pivot.Y;

            var offset = Size * correctedPivot;

            VertexBuffer.Vertices = new[] {
                new Vertex(0 - offset.X,          0 - offset.Y) { Color = Color, TexCoords = new Vector2(0,0) },
                new Vertex(Size.X - offset.X,     0 - offset.Y) { Color = Color, TexCoords = new Vector2(1,0) },
                new Vertex(Size.X - offset.X,     Size.Y - offset.Y) { Color = Color, TexCoords = new Vector2(1,1) },
                new Vertex(0 - offset.X,          Size.Y - offset.Y) { Color = Color, TexCoords = new Vector2(0,1) },
            };
            VertexBuffer.GenerateIndices();
        }
    }
}
