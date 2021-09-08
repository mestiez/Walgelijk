using System.Collections.Generic;

namespace Walgelijk.OpenTK
{
    internal class RenderTargetDictionary
    {
        private Dictionary<RenderTarget, int> handles = new Dictionary<RenderTarget, int>();

        public int Get(RenderTarget target)
        {
            if (handles.TryGetValue(target, out int id))
                return id;

            return -1;
        }

        public void Set(RenderTarget target, int program)
        {
            if (!handles.TryAdd(target, program))
                handles[target] = program;
        }

        public void Delete(RenderTarget target)
        {
            handles.Remove(target);
        }
    }
}
