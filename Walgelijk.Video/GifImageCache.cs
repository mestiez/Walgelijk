using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
            raw.FrameRate = 24;
            return img;
        }

        protected override void DisposeOf(Image<Rgba32> loaded)
        {
            loaded.Dispose();
        }
    }
}
