using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Basic rectangle renderer data
    /// </summary>
    public class RectangleShapeComponent : IBasicShapeComponent
    {
        private Vector2 size = Vector2.One;
        private Color color = Color.White;

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

        public VertexBuffer VertexBuffer { get; internal set; }

        public ShapeRenderTask RenderTask { get; internal set; }

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
            var half = Size / 2;
            VertexBuffer.Vertices = new[] {
                new Vertex(-half.X, -half.Y) { Color = Color, TexCoords = new Vector2(0,0) },
                new Vertex( half.X, -half.Y) { Color = Color, TexCoords = new Vector2(1,0) },
                new Vertex( half.X,  half.Y) { Color = Color, TexCoords = new Vector2(1,1) },
                new Vertex(-half.X,  half.Y) { Color = Color, TexCoords = new Vector2(0,1) },
            };
            VertexBuffer.GenerateIndices();
        }
    }
}
