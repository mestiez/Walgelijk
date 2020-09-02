namespace Walgelijk
{
    /// <summary>
    /// Render task that will set the drawing bounds settings
    /// </summary>
    public class DrawBoundsTask : IRenderTask
    {
        /// <summary>
        /// Draw bounds settings to set
        /// </summary>
        public DrawBounds DrawBounds;

        /// <summary>
        /// Create an instance with the given draw bounds settings
        /// </summary>
        /// <param name="drawBounds"></param>
        public DrawBoundsTask(DrawBounds drawBounds)
        {
            DrawBounds = drawBounds;
        }

        public void Execute(RenderTarget target)
        {
            target.DrawBounds = DrawBounds;
        }
    }
}
