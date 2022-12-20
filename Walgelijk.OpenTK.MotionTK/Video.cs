using FFmpeg.Loader;
using MotionTK;

namespace Walgelijk.OpenTK.MotionTK;

public class VideoSystem : Walgelijk.System
{
    public override void Render()
    {
        foreach (var c in Scene.GetAllComponentsOfType<VideoComponent>())
            c.Video.UpdateAndRender(Graphics);
    }
}

/// <summary>
/// Necessary for a <see cref="Video"/> to be updated by the <see cref="VideoSystem"/>
/// </summary>
public class VideoComponent : Component, IDisposable
{
    public readonly Video Video;

    public VideoComponent(Video video)
    {
        Video = video;
    }

    public void Dispose()
    {
        Video.Dispose();
    }
}

internal readonly struct FfmpegInitialiser
{
    public static bool Initialised = false;

    public static void Initialise()
    {
        if (Initialised)
            return;
        Initialised = true;
        FFmpegLoader
            .SearchApplication()
            .ThenSearchSystem()
            .Load("avformat");    
        FFmpegLoader
            .SearchApplication()
            .ThenSearchSystem()
            .Load("swscale");
    }
}

public class Video : IDisposable
{
    private readonly DataSource source;
    private readonly VideoPlayback video;
    private readonly AudioPlayback audio;

    private readonly PseudoTexture videoTex;
    private readonly Material blitMat;

    public IReadableTexture Texture => videoTex;

    public Video(string path)
    {
        FfmpegInitialiser.Initialise();

        source = new DataSource(path, true, true);
        video = source.VideoPlayback;
        audio = source.AudioPlayback;
        //target = new RenderTexture(video.Size.Width, video.Size.Width, TextureLoader.Settings.WrapMode, TextureLoader.Settings.FilterMode, false, false);
        videoTex = new PseudoTexture(video.TextureHandle, video.Size.Width, video.Size.Width);
        blitMat = new();
    }

    public int Width => videoTex.Width;
    public int Height => videoTex.Height;

    public bool IsPlaying => source.State == PlayState.Playing;
    public bool IsPaused => source.State == PlayState.Paused;
    public bool IsStopped => source.State == PlayState.Stopped;

    /// <summary>
    /// Update the video source
    /// </summary>
    public void UpdateAndRender(IGraphics graphics)
    {
        source.Update();
        //graphics.BlitFullscreenQuad(videoTex, target, target.Width, target.Height, blitMat, ShaderDefaults.MainTextureUniform);
    }

    public void Play() => source.Play();
    public void Pause() => source.Pause();
    public void Stop() => source.Stop();
    public void Restart()
    {
        source.PlayingOffset = TimeSpan.Zero;
        source.Play();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        source.Dispose();
        video.Dispose();
        audio.Dispose();
        //target.Dispose();
        videoTex.Dispose();
        blitMat.Dispose();
    }
}
