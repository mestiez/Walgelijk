using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that sets the appropriate matrices to match a camera
    /// </summary>
    public struct CameraRenderTask : IRenderTask
    {   /// <summary>
        /// View matrix
        /// </summary>
        public Matrix4x4 View { get; set; }

        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix4x4 Projection { get; set; }

        public void Execute(RenderTarget target)
        {
            target.ViewMatrix = View;
            target.ProjectionMatrix = Projection;
        }
    }
}
