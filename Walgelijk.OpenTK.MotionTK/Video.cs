using MotionTK;
using System.Numerics;

namespace Walgelijk.OpenTK.MotionTK;

public class Video : IDisposable
{
    public readonly string SourcePath;

    private DataSource? source;
    private VideoPlayback? video;
    private AudioPlayback? audio;
    private RenderTexture? target;
    private PseudoTexture? videoTex;
    private readonly Material blitMat;

    public event Action? OnReady;

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

    public bool IsReady { get; private set; }

    /// <summary>
    /// Update the video source
    /// </summary>
    public void UpdateAndRender(IGraphics graphics)
    {
        if (source == null || target == null || videoTex == null)
            return;

        source.Update();
        if (!IsReady && source.VideoPlayback.QueuedPackets > 0)
        {
            IsReady = true;
            OnReady?.Invoke();
        }

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
        target = new RenderTexture(video.Size.X, video.Size.Y, TextureLoader.Settings.WrapMode, TextureLoader.Settings.FilterMode, RenderTargetFlags.None);
        videoTex = new PseudoTexture(video.TextureHandle, video.Size.X, video.Size.Y);
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
