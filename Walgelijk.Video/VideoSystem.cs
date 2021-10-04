﻿using System;

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

                    float dt;
                    if (gifComponent.IgnoreTimeScale)
                        dt = Time.UpdateDeltaTimeUnscaled * gifComponent.PlaybackSpeed;
                    else 
                        dt = Time.UpdateDeltaTime * gifComponent.PlaybackSpeed;

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
