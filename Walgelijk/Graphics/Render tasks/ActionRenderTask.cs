using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that will invoke the action you give. Useful for unique smaller actions
    /// </summary>
    public struct ActionRenderTask : IRenderTask
    {
        /// <summary>
        /// Crate action render task
        /// </summary>
        /// <param name="action"></param>
        public ActionRenderTask(Action<RenderTarget> action)
        {
            Action = action;
        }

        /// <summary>
        /// The action to invoke
        /// </summary>
        public Action<RenderTarget> Action { get; set; }

        public void Execute(RenderTarget renderTarget)
        {
            Action?.Invoke(renderTarget);
        }
    }
}
