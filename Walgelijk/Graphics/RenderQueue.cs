using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// This object manages a render queue of <see cref="IRenderTask"/>
    /// </summary>
    public sealed class RenderQueue
    {
        private readonly Queue<IRenderTask> queue = new Queue<IRenderTask>();

        /// <summary>
        /// Render the queue by dequeuing and executing each entry
        /// </summary>
        /// <param name="target"></param>
        public void RenderAndReset(RenderTarget target)
        {
            while (queue.Count != 0)
            {
                var task = queue.Dequeue();
                task.Execute(target);
            }
        }

        /// <summary>
        /// Add a task to the queue
        /// </summary>
        /// <param name="task"></param>
        public void Enqueue(IRenderTask task)
        {
            queue.Enqueue(task);
        }

        /// <summary>
        /// Length of the queue
        /// </summary>
        public int QueueLength => queue.Count;
    }
}
