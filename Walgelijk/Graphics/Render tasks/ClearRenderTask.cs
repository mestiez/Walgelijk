namespace Walgelijk
{
    /// <summary>
    /// Task that clears the target
    /// </summary>
    public class ClearRenderTask : IRenderTask
    {
        /// <summary>
        /// The colour to clear the target with
        /// </summary>
        public Color ClearColor { get; set; } = Colors.Black;

        public ClearRenderTask()
        {

        }

        public ClearRenderTask(Color color)
        {
            ClearColor = color;
        }

        public void Execute(IGraphics graphics)
        {
            graphics.Clear(ClearColor);
        }
    }
}
