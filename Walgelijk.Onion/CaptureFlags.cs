namespace Walgelijk.Onion;

[Flags]
public enum CaptureFlags : byte
{
    /// <summary>
    /// This control cannot be interacted with
    /// </summary>
    None = 0,

    /// <summary>
    /// Will capture hover events
    /// </summary>
    Hover = 1,
    /// <summary>
    /// Will capture scroll events
    /// </summary>
    Scroll = 2,
    /// <summary>
    /// Will capture key events
    /// </summary>
    Key = 4,

    /// <summary>
    /// Will capture all events
    /// </summary>
    All = byte.MaxValue,
}