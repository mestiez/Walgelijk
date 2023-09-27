namespace Walgelijk;

/// <summary>
/// States that a sound can be in
/// </summary>
public enum SoundState
{
    /// <summary>
    /// Untouched by audio engine.
    /// </summary>
    Idle,
    /// <summary>
    /// Currently playing.
    /// </summary>
    Playing,
    /// <summary>
    /// Not playing but not stopped either. This sound can be resumed.
    /// </summary>
    Paused,
    /// <summary>
    /// 
    /// </summary>
    Stopped
}
