using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that will invoke the action you give. Useful for unique smaller actions
    /// </summary>
    public class ActionRenderTask : IRenderTask
    {
        /// <summary>
        /// Crate action render task
        /// </summary>
        public ActionRenderTask(Action<IGraphics> action)
        {
            Action = action;
        }

        /// <summary>
        /// The action to invoke
        /// </summary>
        public Action<IGraphics> Action;

        public void Execute(IGraphics graphics)
        {
            Action?.Invoke(graphics);
        }
    }
}
