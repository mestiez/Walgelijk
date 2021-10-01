namespace Walgelijk.Video
{
    /// <summary>
    /// Component that contains a gif and some options
    /// </summary>
    public class GifComponent
    {
        /// <summary>
        /// Reference to the gif
        /// </summary>
        public Gif Gif;

        public bool IsPlaying = true;
        public float PlaybackTime;
        public float PlaybackSpeed = 1;
        public bool Loop = true;

        /// <summary>
        /// Can be set but it will be useless because it's overwritten all the time
        /// </summary>
        public int CurrentDisplayedFrame = int.MinValue;

        /// <summary>
        /// The texture that is written to. <b>THIS CANNOT BE USED UNTIL <see cref="OnTextureInitialised"/> IS DISPATCHED</b>
        /// </summary>
        public Texture OutputTexture;

        /// <summary>
        /// Event that is dispatched once the system has initialised <see cref="OutputTexture"/>
        /// </summary>
        public Hook<Texture> OnTextureInitialised = new();

        /// <summary>
        /// Set the playback time to 0 and play the animation
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            PlaybackTime = 0;
        }

        /// <summary>
        /// Resume the animation
        /// </summary>
        public void Unpause()
        {
            IsPlaying = true;
        }

        /// <summary>
        /// Pause the animation
        /// </summary>
        public void Pause()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// Stop the animation and set the playback time to 0
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            PlaybackTime = 0;
        }
    }
}
