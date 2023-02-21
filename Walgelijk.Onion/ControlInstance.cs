namespace Walgelijk.Onion;

/// <summary>
/// A control instance. Instances represent each instance of the controls that exist in the tree.
/// They are meant to track the non-hierarchical, general state of a control. 
/// Think of it like a little box attached to a node that contains instance data
/// </summary>
public class ControlInstance
{
    public readonly int Identity;

    /// <summary>
    /// The active rectangles that define the areas on the screen that represent this control for different purposes.
    /// </summary>
    public ControlRects Rects;

    /// <summary>
    /// Amount of seconds that this control will exist for even when no longer being called 
    /// (useful for exit animations)
    /// </summary>
    public float AllowedDeadTime = 0.3f;

    /// <summary>
    /// Is this the currently "selected" control? 
    /// This is different from being Hot or Active because a control can be
    /// focused regardless of mouse position 
    /// </summary>
    public bool HasFocus => Onion.Navigator.FocusedControl == Identity;

    /// <summary>
    /// Determines what events this control is capable of capturing. 
    /// Normally set to just <see cref="CaptureFlags.Cursor"/>, it will only capture hover events.
    /// </summary>
    public CaptureFlags CaptureFlags = CaptureFlags.Cursor;

    public ControlState State
    {
        get
        {
            if (Onion.Navigator.HoverControl == Identity)
                return ControlState.Hot;

            if (Onion.Navigator.ActiveControl == Identity)
                return ControlState.Active;

            return ControlState.None;
        }
    }

    public ControlInstance(int id)
    {
        Identity = id;
    }
}

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
    Cursor = 0b001,
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