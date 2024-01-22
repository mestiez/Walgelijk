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
    /// Has a depth and stencil buffer
    /// </summary>
    [Obsolete("Use DepthStencil")]
    Depth = 1,
    /// <summary>
    /// Has a depth and stencil buffer
    /// </summary>
    DepthStencil = 1,
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
    /// <b>This flag may cause issues when combined with other flags!</b>
    /// </summary>
    Multisampling = 8,
}