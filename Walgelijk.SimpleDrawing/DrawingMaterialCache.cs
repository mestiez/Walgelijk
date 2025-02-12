using System;
using System.Collections.Generic;

namespace Walgelijk.SimpleDrawing
{
    /// <summary>
    /// Caches textures bound to materials
    /// </summary>
    public class DrawingMaterialCache : Cache<IReadableTexture, Material>
    {
        /// <summary>
        /// A cached material will be disposed if it is not requested for this many frames
        /// </summary>
        public static int MaterialTTL = 60;

        protected override Material CreateNew(IReadableTexture raw) => DrawingMaterialCreator.Create(raw);

        private readonly Dictionary<IReadableTexture, RefCounter> useCounter = [];
        private readonly Stack<IReadableTexture> toDispose = [];

        public void CleanExpired()
        {
            foreach (var item in useCounter)
            {
                if (item.Value.DeadTime >= MaterialTTL)
                    toDispose.Push(item.Key);

                if (item.Value.Used)
                    item.Value.DeadTime = 0;
                else
                    item.Value.DeadTime++;

                item.Value.Used = false;
            }

            while (toDispose.TryPop(out var p))
            {
                useCounter.Remove(p);
                Unload(p);
            }
        }

        public override Material Load(IReadableTexture obj)
        {
            useCounter.Ensure(obj).Used = true;
            return base.Load(obj);
        }

        protected override void DisposeOf(Material loaded)
        {
            loaded.Dispose();
        }

        private class RefCounter
        {
            public bool Used;
            public int DeadTime;
        }
    }
}