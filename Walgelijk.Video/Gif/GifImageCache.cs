using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Walgelijk.Video
{
    internal class GifImageCache : Cache<Gif, Image<Rgba32>>
    {
        protected override Image<Rgba32> CreateNew(Gif raw)
        {
            var img = Image.Load<Rgba32>(raw.Path);
            raw.Width = img.Width;
            raw.Height = img.Height;
            raw.FrameCount = img.Frames.Count;

            raw.DurationInSeconds = 0;
            raw.FrameDelays = new float[img.Frames.Count];

            float lastFrameDelay = Gif.DefaultDelayPerFrame;

            int i = 0;
            foreach (var item in img.Frames)
            {
                var delayInSeconds = item.Metadata.GetGifMetadata().FrameDelay * 0.01f;

                if (delayInSeconds <= float.Epsilon)
                    delayInSeconds = lastFrameDelay;
                else
                    lastFrameDelay = delayInSeconds;

                raw.FrameDelays[i] = delayInSeconds;
                raw.DurationInSeconds += MathF.Max(delayInSeconds, Gif.DefaultDelayPerFrame);

                i++;
            }

            return img;
        }

        protected override void DisposeOf(Image<Rgba32> loaded)
        {
            loaded.Dispose();
        }
    }
}
