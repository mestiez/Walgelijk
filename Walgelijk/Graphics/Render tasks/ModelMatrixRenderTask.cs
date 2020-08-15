using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that sets the model matrix
    /// </summary>
    public struct ModelMatrixRenderTask : IRenderTask
    {
        public ModelMatrixRenderTask(Matrix4x4 matrix)
        {
            Matrix = matrix;
        }

        /// <summary>
        /// The model matrix
        /// </summary>
        public Matrix4x4 Matrix { get; set; }

        public void Execute(RenderTarget target)
        {
            // TODO model matrix shit
        }
    }
}
