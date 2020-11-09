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

        public void Execute(IGraphics graphics)
        {
            Game.Main.Window.Graphics.DrawBounds = DrawBounds;
        }

        /// <summary>
        /// Task that disabled the drawbounds. This instance is <b>shared</b> and should not be changed.
        /// </summary>
        public static readonly DrawBoundsTask DisableDrawBoundsTask = new DrawBoundsTask(DrawBounds.DisabledBounds);
    }
}
