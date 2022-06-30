namespace Walgelijk
{
    /// <summary>
    /// Component that holds camera data
    /// </summary>
    [RequiresComponents(typeof(TransformComponent))]
    public class CameraComponent
    {
        /// <summary>
        /// The orthographic size of this camera
        /// </summary>
        public float OrthographicSize { get; set; } = 1;

        /// <summary>
        /// Amount of pixels per translation unit
        /// </summary>
        public float PixelsPerUnit { get; set; } = 100;

        /// <summary>
        /// The order of this camera's tasks. <see cref="int.MinValue"/> by default
        /// </summary>
        public int TaskOrder { get; set; } = int.MinValue;

        /// <summary>
        /// Colour to clear with
        /// </summary>
        public Color ClearColour { get; set; } = Colors.Black;

        /// <summary>
        /// Should this camera clear the target before rendering?
        /// </summary>
        public bool Clear { get; set; } = true;
    }
}
