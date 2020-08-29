using System;

namespace Walgelijk
{
    /// <summary>
    /// Component that holds camera data
    /// </summary>
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
    }
}
