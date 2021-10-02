namespace Walgelijk.Video
{
    public static class GifManager
    {
        internal static readonly GifImageCache Cache = new();

        public static void Preload(Gif gif)
        {
            Cache.Load(gif);
        }

        public static Texture InitialiseTextureFor(Gif gif)
        {
            var data = Cache.Load(gif);
            if (data == null)
                return Texture.ErrorTexture;
            return new Texture(gif.Width, gif.Height, new Color[gif.Height * gif.Width], false);
        }

        public static void GetFrame(Gif gif, int index, Texture output)
        {
            var i = Cache.Load(gif);
            var frame = i.Frames[index];
            TextureLoader.CopyPixels(frame, ref output.RawData, true);
            output.ForceUpdate();
        }
    }
}
