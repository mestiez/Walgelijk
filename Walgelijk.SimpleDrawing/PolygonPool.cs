namespace Walgelijk.SimpleDrawing
{
    /// <summary>
    /// The pool that polygon vertex buffers are taken out of
    /// </summary>
    public class PolygonPool : Pool<VertexBuffer?, bool>
    {
        public PolygonPool(int maxCapacity = 1000) : base(maxCapacity)
        {
        }

        protected override VertexBuffer? CreateFresh()
        {
            return new VertexBuffer();
        }

        protected override VertexBuffer? GetOverCapacityFallback()
        {
            return null;
        }

        protected override void ResetObjectForNextUse(VertexBuffer? obj, bool initialiser)
        {
            obj.AmountOfIndicesToRender = null;
            obj.Dynamic = true;
        }
    }
}