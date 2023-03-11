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
    Hover = 0b001,
    /// <summary>
    /// Will capture scroll events
    /// </summary>
    Scroll = 0b010,   
    /// <summary>
    /// Will capture key events
    /// </summary>
    Key    = 0b100,

    /// <summary>
    /// Will capture all events
    /// </summary>
    All = byte.MaxValue,
}