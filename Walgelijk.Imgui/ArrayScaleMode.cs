namespace Walgelijk.Imgui
{
    /// <summary>
    /// Other axis scaling mode
    /// </summary>
    public enum ArrayScaleMode
    {
        /// <summary>
        /// Keep original size
        /// </summary>
        None,
        /// <summary>
        /// Stretch to 100% of the container width
        /// </summary>
        Stretch,
        /// <summary>
        /// Scale down the control if it exceeds the size of its container
        /// </summary>
        Contain
    }
}
