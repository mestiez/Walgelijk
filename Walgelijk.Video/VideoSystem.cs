using System;

namespace Walgelijk.Video
{
    /// <summary>
    /// Controls the behaviour of gifs and videos
    /// </summary>
    public class VideoSystem : Walgelijk.System
    {
        public override void Update()
        {
            UpdateGifs();
        }

        private void UpdateGifs()
        {
            foreach (var item in Scene.GetAllComponentsOfType<GifComponent>())
            {
                var gifComponent = item.Component;
                if (gifComponent.Gif == null)
                    continue;

                if (gifComponent.IsPlaying)
                {
                    var gif = gifComponent.Gif;
                    if (gifComponent.OutputTexture == null)
                    {
                        gifComponent.OutputTexture = GifManager.InitialiseTextureFor(gif);
                        gifComponent.OnTextureInitialised?.Dispatch(gifComponent.OutputTexture);
                    }

                    var dt = Time.UpdateDeltaTime * gifComponent.PlaybackSpeed;
                    if (gifComponent.IgnoreTimeScale)
                        dt /= Time.TimeScale;

                    gifComponent.PlaybackTime += dt;
                        
                    int frameToDisplay = gif.GetFrameIndexAt(gifComponent.PlaybackTime);

                    if (!gifComponent.Loop && frameToDisplay >= gif.FrameCount - 1)
                        gifComponent.IsPlaying = false;

                    if (frameToDisplay == gifComponent.CurrentDisplayedFrame)
                        continue;

                    gifComponent.CurrentDisplayedFrame = frameToDisplay;

                    GifManager.GetFrame(gif, frameToDisplay, gifComponent.OutputTexture);
                }
            }
        }
    }
}
