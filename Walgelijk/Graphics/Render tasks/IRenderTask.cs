using System.Collections.Generic;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// A task that can be queued in the render queue
    /// </summary>
    public interface IRenderTask
    {
        /// <summary>
        /// Execute the render task on the given render target
        /// </summary>
        /// <param name="target"></param>
        public void Execute(RenderTarget target);
    }
}
