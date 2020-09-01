namespace Walgelijk
{
    /// <summary>
    /// An ordered group of render tasks that should always be executed together
    /// </summary>
    public class GroupRenderTask : IRenderTask
    {
        /// <summary>
        /// The group of render tasks
        /// </summary>
        public IRenderTask[] Tasks;

        public void Execute(RenderTarget target)
        {
            if (Tasks == null) return;
            for (int i = 0; i < Tasks.Length; i++)
                Tasks[i].Execute(target);
        }
    }
}
