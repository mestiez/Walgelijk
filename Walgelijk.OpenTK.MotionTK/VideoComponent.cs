namespace Walgelijk.OpenTK.MotionTK;

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
