namespace Walgelijk.Video
{
    public static class GifManager
    {
        internal static readonly GifImageCache Cache = new();

        public static Texture InitialiseTextureFor(Gif video)
        {
            var data = Cache.Load(video);
            if (data == null)
                return Texture.ErrorTexture;
            return new Texture(video.Width, video.Height, new Color[video.Height * video.Width], false);
        }

        public static void GetFrame(Gif video, int index, Texture output)
        {
            var i = Cache.Load(video);
            var frame = i.Frames[index];
            TextureLoader.CopyPixels(frame, ref output.RawData, true);
            output.ForceUpdate();
        }
    }
}
