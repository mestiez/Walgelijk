using System;

namespace Walgelijk.Video
{
    /// <summary>
    /// Object that represents an animated gif image 
    /// </summary>
    public class Gif : IDisposable
    {
        public readonly string Path;
        public int Height;
        public int Width;
        public int FrameCount;
        public int FrameRate = 24;

        public Gif(string path)
        {
            Path = path;
        }

        public void Dispose()
        {
            GifManager.Cache.Unload(this);
            GC.SuppressFinalize(this);
        }
    }
}
