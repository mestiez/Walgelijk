namespace Walgelijk.Onion;

/// <summary>
/// A control instance. Instances represent each instance of the controls that exist in the tree.
/// They are meant to track the non-hierarchical, general state of a control.
/// </summary>
public class ControlInstance
{
    public readonly int Identity;

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

    public ControlState State
    {
        get
        {
            if (Onion.Navigator.HotControl == Identity)
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

public enum ControlState
{
    /// <summary>
    /// Inactive state (idle)
    /// </summary>
    None,
    /// <summary>
    /// The user is probably about to interact with this control
    /// </summary>
    Hot,
    /// <summary>
    /// The user is currently interacting with this control
    /// </summary>
    Active,
}

public struct ControlRects
{
    /// <summary>
    /// The preferred rectangle for this control, as adjusted before rendering
    /// </summary>
    public Rect Target;

    /// <summary>
    /// The rectangle that is considered for raycasting.
    /// The control won't be considered at all if this is null.
    /// </summary>
    public Rect? Raycast;

    /// <summary>
    /// The rectangle that encapsulates all children of this control in local space.
    /// This determines the scrollable area within this control
    /// </summary>
    public Rect ChildContent;

    /// <summary>
    /// The final rectangle that represents the rendered area of this control on the window.
    /// Do note that this rectangle may be obstructed by other rectangles, so this is not 
    /// representative of the currently visible area of this control, 
    /// only the area that is actively being rendered.
    /// </summary>
    public Rect Rendered;
}