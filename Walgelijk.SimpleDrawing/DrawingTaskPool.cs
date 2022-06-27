
using System.Numerics;

namespace Walgelijk.SimpleDrawing
{
    /// <summary>
    /// The pool that drawing tasks are taken out of
    /// </summary>
    public class DrawingTaskPool : Pool<PooledDrawingTask, Drawing>
    {
        public DrawingTaskPool(int maxCapacity = 1000) : base(maxCapacity)
        {
        }

        protected override PooledDrawingTask CreateFresh() => new PooledDrawingTask(default);

        protected override PooledDrawingTask? GetOverCapacityFallback() => null;

        protected override void ResetObjectForNextUse(PooledDrawingTask obj, Drawing initialiser)
        {
            obj.Drawing = initialiser;
        }
    }

    public class TextMeshCache : Cache<CachableTextDrawing, VertexBuffer>
    {
        public static void Prepare(CachableTextDrawing drawing, VertexBuffer vertexBuffer, bool dynamic = false)
        {
            vertexBuffer.Dynamic = dynamic;

            int vertexCount = drawing.Text.Length * 4;
            int indexCount = drawing.Text.Length * 6;

            if (vertexBuffer.Vertices == null || vertexBuffer.Vertices.Length < vertexCount)
            {
                vertexBuffer.Vertices = new Vertex[vertexCount];
                vertexBuffer.Indices = new uint[indexCount];
                Logger.Debug("Created new arrays for text mesh");
            }

            Draw.TextMeshGenerator.Font = drawing.Font ?? Font.Default;
            Draw.TextMeshGenerator.Color = drawing.Color;
            Draw.TextMeshGenerator.VerticalAlign = drawing.VerticalAlign;
            Draw.TextMeshGenerator.HorizontalAlign = drawing.HorizontalAlign;
            Draw.TextMeshGenerator.WrappingWidth = drawing.TextBoxWidth;

            var result = Draw.TextMeshGenerator.Generate(drawing.Text, vertexBuffer.Vertices, vertexBuffer.Indices);
            vertexBuffer.AmountOfIndicesToRender = result.IndexCount;
            vertexBuffer.ForceUpdate();
        }

        protected override VertexBuffer CreateNew(CachableTextDrawing drawing)
        {
            int vertexCount = drawing.Text.Length * 4;
            int indexCount = drawing.Text.Length * 6;

            var vertexBuffer = new VertexBuffer(new Vertex[vertexCount], new uint[indexCount]);

            Prepare(drawing, vertexBuffer, false);
            Logger.Debug("Cached common string for text mesh generation");

            return vertexBuffer;
        }

        protected override void DisposeOf(VertexBuffer loaded)
        {
            loaded.Dispose();
        }
    }
}