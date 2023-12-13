using System;

namespace Walgelijk;

/// <summary>
/// Render texture features
/// </summary>
[Flags]
public enum RenderTextureFlags
{
    /// <summary>
    /// Nothing special
    /// </summary>
    None = 0,
    /// <summary>
    /// Has a depth buffer
    /// </summary>
    Depth = 1,
    /// <summary>
    /// Allows HDR values
    /// </summary>
    HDR = 2,
    /// <summary>
    /// Generates mipmaps
    /// </summary>
    Mipmaps = 4,
    /// <summary>
    /// Multiple samples per pixel (MSAA)
    /// </summary>
    Multisampling = 8
}