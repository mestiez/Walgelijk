using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Basic rectangle renderer data
    /// </summary>
    public class RectangleRendererComponent : IBasicShapeComponent
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
        public RectangleRendererComponent()
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
            VertexBuffer.Vertices = new[] {
                new Vertex(0, 0) { Color = Color },
                new Vertex(Size.X, 0){ Color = Color },
                new Vertex(Size.X, Size.Y){ Color = Color },
                new Vertex(0, Size.Y){ Color = Color },
            };
            VertexBuffer.GenerateIndices();
        }
    }
}
