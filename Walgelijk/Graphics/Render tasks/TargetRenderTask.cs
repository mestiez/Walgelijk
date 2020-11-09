namespace Walgelijk
{
    /// <summary>
    /// Task that sets the active render target
    /// </summary>
    public class TargetRenderTask : IRenderTask
    {
        /// <summary>
        /// The target to set
        /// </summary>
        public RenderTarget Target { get; set; }

        public void Execute(IGraphics graphics)
        {
            if (graphics.CurrentTarget != Target)
                graphics.CurrentTarget = Target;
        }
    }
}
