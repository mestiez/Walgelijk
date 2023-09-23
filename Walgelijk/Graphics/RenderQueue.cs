using System.Collections.Generic;

namespace Walgelijk
{
    /// <summary>
    /// This object manages a render queue of <see cref="IRenderTask"/>
    /// </summary>
    public sealed class RenderQueue
    {
        private readonly List<Command> commands = new();

        /// <summary>
        /// Render the queue by dequeuing and executing each entry
        /// </summary>
        public void RenderAndReset(IGraphics graphics)
        {
            for (int i = 0; i < commands.Count; i++)
                commands[i].RenderTask.Execute(graphics);
            commands.Clear();
        }

        /// <summary>
        /// Add a task to the queue. The optional order determines when it's going to be executed. Higher values mean later execution.
        /// </summary>
        public void Add(IRenderTask task, RenderOrder order = default)
        {
            var command = new Command(task, order);

            if (commands.Count == 0 || commands[^1].Order <= order)
            {
                commands.Add(command);
                return;
            }

            ReverseSortAdd(command);
        }

        private void ReverseSortAdd(Command command)
        {
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                if (commands[i].Order <= command.Order)
                {
                    commands.Insert(i + 1, command);
                    return;
                }
            }

            //er is geen command die een lagere order heeft dus deze moet aan het begin
            commands.Insert(0, command);
        }

        /// <summary>
        /// Length of the queue
        /// </summary>
        public int Length => commands.Count;

        private readonly struct Command
        {
            public readonly IRenderTask RenderTask;
            public readonly RenderOrder Order;

            public Command(IRenderTask renderTask, RenderOrder order)
            {
                RenderTask = renderTask;
                Order = order;
            }
        }
    }
}
