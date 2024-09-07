namespace Walgelijk;

/// <summary>
/// Blend modes
/// </summary>
public enum BlendMode
{
    /// <summary>
    /// Default blend mode
    /// </summary>
    AlphaBlend,
    /// <summary>
    /// Adds RGB values
    /// </summary>
    Addition,
    /// <summary>
    /// Invert, multiply, invert
    /// </summary>
    Screen,
    /// <summary>
    /// Multiply blend mode
    /// </summary>
    Multiply,
    /// <summary>
    /// Only keeps the greatest pixel value
    /// </summary>
    Lighten,
    /// <summary>
    /// Only keeps the smallest pixel value
    /// </summary>
    Darken,
    /// <summary>
    /// Only keeps the smallest pixel value
    /// </summary>
    Overlay,
    /// <summary>
    /// Inverts pixel values
    /// </summary>
    Negate,
    /// <summary>
    /// Overwrite blend mode (disables blending)
    /// </summary>
    Overwrite
}
