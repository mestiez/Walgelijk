using System;

namespace Walgelijk.Video
{
    /// <summary>
    /// Object that represents an animated gif image 
    /// </summary>
    public class Gif : IDisposable
    {
        public const float DefaultDelayPerFrame = 1 / 24f;

        public readonly string Path;
        public int Height;
        public int Width;
        public int FrameCount;
        public float DurationInSeconds;

        public float[] FrameDelays;

        public Gif(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Prepare a gif to be loaded and played without actually reading the file. This is faster but it only postpones actually reading the file. Use <see cref="GifManager.Preload(Gif)"/> to forcibly load a gif
        /// </summary>
        public static Gif LoadLazy(string path) => new(path);

        /// <summary>
        /// Load a gif immediately, reading all frames and creating all the required objects
        /// </summary>
        public static Gif Load(string path)
        {
            var gif = new Gif(path);
            GifManager.Preload(gif);
            return gif;
        }

        public void Dispose()
        {
            GifManager.Cache.Unload(this);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get the frame at any given point in time
        /// </summary>
        public int GetFrameIndexAt(float playbackTimeInSeconds)
        {
            float t = 0;
            for (int i = 0; i < FrameCount; i++)
            {
                t += MathF.Max(FrameDelays[i], DefaultDelayPerFrame);
                if (t >= playbackTimeInSeconds % DurationInSeconds)
                    return i;
            }

            return FrameCount - 1;
        }
    }
}
