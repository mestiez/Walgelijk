using System;

namespace Walgelijk
{
    /// <summary>
    /// Simple stucture that holds data for the layer system
    /// </summary>
    public struct RenderOrder
    {
        /// <summary>
        /// The main layer to draw on
        /// </summary>
        public int Layer;
        /// <summary>
        /// The order within <see cref="Layer"/> to draw on
        /// </summary>
        public int OrderInLayer;

        /// <summary>
        /// Constructs a rendering order structure
        /// </summary>
        public RenderOrder(int layer, int orderInLayer = 0)
        {
            Layer = layer;
            OrderInLayer = orderInLayer;
        }

        /// <summary>
        /// Return a <see cref="RenderOrder"/> on the same layer, but with the given order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public RenderOrder WithOrder(int order)
        {
            return new RenderOrder(Layer, order);
        }

        public override bool Equals(object? obj)
        {
            return obj is RenderOrder order &&
                   Layer == order.Layer &&
                   OrderInLayer == order.OrderInLayer;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Layer, OrderInLayer);
        }

        public static RenderOrder operator +(RenderOrder left, RenderOrder right)
        {
            return new RenderOrder(left.Layer + right.Layer, left.OrderInLayer + right.OrderInLayer);
        }

        public static RenderOrder operator -(RenderOrder left, RenderOrder right)
        {
            return new RenderOrder(left.Layer - right.Layer, left.OrderInLayer - right.OrderInLayer);
        }

        public static bool operator ==(RenderOrder left, RenderOrder right)
        {
            return left.OrderInLayer == right.OrderInLayer && left.Layer == right.Layer;
        }

        public static bool operator !=(RenderOrder left, RenderOrder right)
        {
            return !(left == right);
        }

        public static bool operator >(RenderOrder left, RenderOrder right)
        {
            if (left.Layer == right.Layer)
                return left.OrderInLayer > right.OrderInLayer;
            else
                return left.Layer > right.Layer;
        }

        public static bool operator <(RenderOrder left, RenderOrder right)
        {
            if (left.Layer == right.Layer)
                return left.OrderInLayer < right.OrderInLayer;
            else
                return left.Layer < right.Layer;
        }

        public static bool operator >=(RenderOrder left, RenderOrder right)
        {
            if (left.Layer == right.Layer)
                return left.OrderInLayer >= right.OrderInLayer;
            else
                return left.Layer >= right.Layer;
        }

        public static bool operator <=(RenderOrder left, RenderOrder right)
        {
            if (left.Layer == right.Layer)
                return left.OrderInLayer <= right.OrderInLayer;
            else
                return left.Layer <= right.Layer;
        }
    }
}
