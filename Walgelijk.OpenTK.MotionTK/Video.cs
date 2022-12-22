using FFmpeg.Loader;
using MotionTK;
using System.Numerics;

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

        //FFmpeg.AutoGen.ffmpeg.swr_alloc_set_opts(_swrCtx,
        //        AV_CH_LAYOUT_STEREO,
        //        AV_SAMPLE_FMT_S16,
        //        48000,
        //        (long)_audioCtx->channel_layout,
        //        _audioCtx->sample_fmt,
        //        _audioCtx->sample_rate,
        //        0, null);

    }
}

public class Video : IDisposable
{
    public readonly string SourcePath;

    private DataSource? source;
    private VideoPlayback? video;
    private AudioPlayback? audio;
    private RenderTexture? target;
    private PseudoTexture? videoTex;
    private readonly Material blitMat;

    public IReadableTexture Texture => target ?? (IReadableTexture)Walgelijk.Texture.ErrorTexture;

    public Video(string path)
    {
        FfmpegInitialiser.Initialise();

        SourcePath = path;
        blitMat = new();

        Initialise();
    }

    public int Width => videoTex?.Width ?? 0;
    public int Height => videoTex?.Height ?? 0;

    public bool IsPlaying => source?.State == PlayState.Playing;
    public bool IsPaused => source?.State == PlayState.Paused;
    public bool IsStopped => source?.State == PlayState.Stopped;
    public TimeSpan Time => source?.PlayingOffset ?? TimeSpan.Zero;
    public TimeSpan Duration => source?.FileLength ?? TimeSpan.Zero;

    /// <summary>
    /// Update the video source
    /// </summary>
    public void UpdateAndRender(IGraphics graphics)
    {
        if (source == null || target == null || videoTex == null)
            return;

        source.Update();
        {
            blitMat.SetUniform(ShaderDefaults.MainTextureUniform, videoTex);
            target.ModelMatrix = Matrix4x4.CreateTranslation(0, 0, 0) * Matrix4x4.CreateScale(target.Width, target.Height, 1);

            var view = target.ViewMatrix;
            var proj = target.ProjectionMatrix;
            target.ProjectionMatrix = target.OrthographicMatrix;
            target.ViewMatrix = Matrix4x4.Identity;

            graphics.CurrentTarget = target;
            graphics.Draw(PrimitiveMeshes.Quad, blitMat);
            target.ViewMatrix = view;
            target.ProjectionMatrix = proj;
        }
    }

    private void Initialise()
    {
        source?.Dispose();
        video?.Dispose();
        audio?.Dispose();
        target?.Dispose();
        videoTex?.Dispose();

        source = new DataSource(SourcePath, true, true);
        video = source.VideoPlayback;
        audio = source.AudioPlayback;
        target = new RenderTexture(video.Size.Width, video.Size.Height, TextureLoader.Settings.WrapMode, TextureLoader.Settings.FilterMode, false, false);
        videoTex = new PseudoTexture(video.TextureHandle, video.Size.Width, video.Size.Height);
    }

    public void Play() => source?.Play();
    public void Pause() => source?.Pause();
    public void Stop() => source?.Stop();
    public void Restart()
    {
        Initialise();
        source?.Play();
    }

    ~Video() => Dispose();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        source?.Dispose();
        video?.Dispose();
        audio?.Dispose();
        target?.Dispose();
        videoTex?.Dispose();
        blitMat.Dispose();
    }
}
