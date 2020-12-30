namespace Walgelijk
{
    /// <summary>
    /// Structure that holds some common <see cref="RenderOrder"/>s
    /// </summary>
    public struct DefaultLayers
    {
        /// <summary>
        /// The default render order. (0, 0)
        /// </summary>
        public static readonly RenderOrder Zero = new RenderOrder();

        /// <summary>
        /// The render order where camera operations are executed (-10000, 0)
        /// </summary>
        public static readonly RenderOrder CameraOperations = new RenderOrder(-10000, 0);

        /// <summary>
        /// The render order where UI is drawn (10000, 0)
        /// </summary>
        public static readonly RenderOrder UI = new RenderOrder(10000, 0);

        /// <summary>
        /// The render order where debugging UI is drawn (10001, 0)
        /// </summary>
        public static readonly RenderOrder DebugUI = new RenderOrder(10001, 0);

        /// <summary>
        /// The minimum render order (<see cref="int.MinValue"/>, <see cref="int.MinValue"/>)
        /// </summary>
        public static readonly RenderOrder Bottom = new RenderOrder(int.MinValue, int.MinValue);

        /// <summary>
        /// The maximum render order (<see cref="int.MaxValue"/>, <see cref="int.MaxValue"/>)
        /// </summary>
        public static readonly RenderOrder Top = new RenderOrder(int.MaxValue, int.MaxValue);
    }
}
