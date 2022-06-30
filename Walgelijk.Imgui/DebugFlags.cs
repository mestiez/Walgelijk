#pragma warning disable CA2211 // Non-constant fields should not be visible
namespace Walgelijk.Imgui;

/// <summary>
/// Debug flags for <see cref="Gui.DebugFlags"/>
/// </summary>
[global::System.Flags]
public enum DebugFlags
{
    /// <summary>
    /// Debug mode off
    /// </summary>
    None = 0,
    /// <summary>
    /// Draw draw bounds
    /// </summary>
    DrawBounds = 0b00001,
    /// <summary>
    /// Draw raycast rect
    /// </summary>
    RaycastRect = 0b00010,
    /// <summary>
    /// Draw a rectangle around controls that the mouse is hovering over
    /// </summary>
    RaycastHit = 0b00100,
    /// <summary>
    /// Highlight the control that is eating scroll input
    /// </summary>
    ScrollEater = 0b01000,
    /// <summary>
    /// Highlight the control's bounds
    /// </summary>
    Bounds = 0b10000,
}
#pragma warning restore CA2211 // Non-constant fields should not be visible
