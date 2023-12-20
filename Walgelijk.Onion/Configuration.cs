namespace Walgelijk.Onion;

public class Configuration
{
    public int RenderLayer = RenderOrder.UI.Layer;
    public float ScrollSensitivity = 18;
    public Key ScrollHorizontal = Key.LeftShift;
    public float SmoothScroll = 0;
    public TimeSpan DoubleClickTimeWindow = TimeSpan.FromSeconds(0.2);
    public bool ProcessCursorStack = true;

    public float SoundVolume = 0.5f;
    public AudioTrack? AudioTrack;
}