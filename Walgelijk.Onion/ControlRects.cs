namespace Walgelijk.Onion;

public struct ControlRects
{
    /// <summary>
    /// The target rectangle in local (parent) space
    /// </summary>
    public Rect Local;

    /// <summary>
    /// During the calculation of the <see cref="ComputedGlobal"/> rectangle, this rectangle is used
    /// </summary>
    public Rect Intermediate;

    /// <summary>
    /// The rectangle derived from <see cref="Local"/> after layout processing, transformed to global space
    /// <br></br>
    /// Defined in global (screen) space
    /// </summary>
    public Rect ComputedGlobal;

    /// <summary>
    /// The final rectangle that represents the rendered area of this control on the window.
    /// Do note that this rectangle may be obstructed by other rectangles, so this is not 
    /// representative of the currently visible area of this control, 
    /// only the area that is actively being rendered.
    /// <br></br>
    /// Defined in global (screen) space
    /// </summary>
    public Rect Rendered;

    /// <summary>
    /// The rectangle that is considered for raycasting and event capturing.
    /// Can be null, in which case the control is skipped during a raycast and no events will ever be captured.
    /// <br></br>
    /// Defined in global (screen) space
    /// </summary>
    public Rect? Raycast;

    /// <summary>
    /// The rectangle that encapsulates all children of this control in local space.
    /// This determines the scrollable area within this control
    /// <br></br>
    /// Defined in local (parent) space
    /// </summary>
    public Rect ChildContent;

    /// <summary>
    /// The rectangle that is determined by the minimum and maximum values of <see cref="ControlInstance.InnerScrollOffset"/>
    /// <br></br>
    /// Defined in local (parent) space
    /// </summary>
    public Rect ComputedScrollBounds;

    /// <summary>
    /// This rectangle determines the area in which it and its children can be drawn. 
    /// Note that this may not represent the final bounding box because it will might be cutoff by its parent bounding box.
    /// See <see cref="ComputedDrawBounds"/> for the final draw bounds.
    /// <br></br>
    /// If this is null it will fall back to the parent drawbounds, effectively having no influence on the drawbound chain.
    /// <br></br>
    /// Defined in global (screen) space
    /// </summary>
    public Rect? DrawBounds;

    /// <summary>
    /// The draw bounds that should be considered when drawing the control. It is derived from <see cref="DrawBounds"/> and the bounds of all its parents.
    /// It is never larger than <see cref="DrawBounds"/>.
    /// <br></br>
    /// Defined in global (screen) space
    /// </summary>
    public Rect ComputedDrawBounds;
}