using System.Numerics;

namespace Walgelijk
{
    public abstract class RenderTarget
    {
        public abstract Vector2 Size { get; set; }
        public abstract Color ClearColour { get; set; }

        public abstract void Clear();
        public abstract void Draw(VertexBuffer vertexBuffer, Material material);
    }
}
