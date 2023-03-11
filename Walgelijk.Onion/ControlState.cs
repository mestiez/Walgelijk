namespace Walgelijk.Onion;

[Flags]
public enum ControlState : byte
{
    /// <summary>
    /// Inactive state (idle)
    /// </summary>
    None = 0,
    /// <summary>
    /// The cursor is hovering over this control. <see cref="Navigator.HoverControl"/>
    /// </summary>
    Hover = 1,
    /// <summary>
    /// The user is scrolling over this control. <see cref="Navigator.ScrollControl"/>
    /// </summary>
    Scroll = 2,
    /// <summary>
    /// This control is selected. <see cref="Navigator.FocusedControl"/>
    /// </summary>
    Focus = 4,
    /// <summary>
    /// The user is actively interacting with this control. <see cref="Navigator.ActiveControl"/>
    /// </summary>
    Active = 8,
}
