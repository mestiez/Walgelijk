using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that sets the appropriate matrices to match a camera
    /// </summary>
    public class CameraRenderTask : IRenderTask
    {   /// <summary>
        /// View matrix
        /// </summary>
        public Matrix4x4 View;

        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix4x4 Projection;

        public void Execute(IGraphics graphics)
        {
            graphics.CurrentTarget.ViewMatrix = View;
            graphics.CurrentTarget.ProjectionMatrix = Projection;
        }
    }
}
