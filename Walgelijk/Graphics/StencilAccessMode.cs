namespace Walgelijk;

/// <summary>
/// Stencil access mode. Defines ways for drawn geometry to interact with the stencil buffer
/// </summary>
public enum StencilAccessMode
{
    // glStencilMask(0x00);
    /// <summary>
    /// Only read from the stencil buffer
    /// </summary>
    NoWrite,
    // glStencilMask(0xFF);
    /// <summary>
    /// Write 1 to the stencil buffer
    /// </summary>
    Write,
}
