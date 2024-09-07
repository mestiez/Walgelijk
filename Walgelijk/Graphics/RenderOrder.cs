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

        /// <summary>
        /// Return a copy of this RenderOrder with the layer offset by the given amount
        /// </summary>
        public RenderOrder OffsetLayer(int offset)
        {
            return new RenderOrder(Layer + offset, OrderInLayer);
        }

        /// <summary>
        /// Return a copy of this RenderOrder with the order offset by the given amount
        /// </summary>
        public RenderOrder OffsetOrder(int offset)
        {
            return new RenderOrder(Layer, OrderInLayer + offset);
        }

        /// <summary>
        /// Return a copy of this RenderOrder with both the layer and order offset by the given amount
        /// </summary>
        public RenderOrder Offset(int layerOffset, int orderOffset)
        {
            return new RenderOrder(Layer + layerOffset, OrderInLayer + orderOffset);
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

        /// <summary>
        /// The default render order. (0, 0)
        /// </summary>
        public static readonly RenderOrder Zero = new RenderOrder();

        /// <summary>
        /// The render order where camera operations are executed (-10000, 0)
        /// </summary>
        public static readonly RenderOrder CameraOperations = new(-10000, 0);

        /// <summary>
        /// The render order where UI is drawn (10000, 0)
        /// </summary>
        public static readonly RenderOrder UI = new(10000, 0);

        /// <summary>
        /// The render order where debugging UI is drawn (10001, 0)
        /// </summary>
        public static readonly RenderOrder DebugUI = new(10001, 0);

        /// <summary>
        /// The minimum render order (<see cref="int.MinValue"/>, <see cref="int.MinValue"/>)
        /// </summary>
        public static readonly RenderOrder Bottom = new(int.MinValue, int.MinValue);

        /// <summary>
        /// The maximum render order (<see cref="int.MaxValue"/>, <see cref="int.MaxValue"/>)
        /// </summary>
        public static readonly RenderOrder Top = new(int.MaxValue, int.MaxValue);
    }
}
