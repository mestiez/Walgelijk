namespace Walgelijk.Onion;

public struct ControlRects
{
    /// <summary>
    /// The preferred rectangle for this control, as adjusted before rendering
    /// </summary>
    public Rect Target;

    /// <summary>
    /// The rectangle that is considered for raycasting and event capturing.
    /// Can be null, in which case the control is skipped during a raycast and no events will ever be captured.
    /// </summary>
    public Rect? Raycast;

    /// <summary>
    /// The rectangle that encapsulates all children of this control in local space.
    /// This determines the scrollable area within this control
    /// </summary>
    public Rect ChildContent;

    /// <summary>
    /// This rectangle determines the area in which it and its children can be drawn. 
    /// Note that this may not represent the final bounding box because it will might be cutoff by its parent bounding box.
    /// See <see cref="ComputedDrawBounds"/> for the final draw bounds.
    /// <br></br>
    /// If this is null it will fall back to the parent drawbounds, effectively having no influence on the drawbound chain.
    /// </summary>
    public Rect? DrawBounds;

    /// <summary>
    /// The draw bounds that should be considered when drawing the control. It is derived from <see cref="DrawBounds"/> and the bounds of all its parents.
    /// It is never larger than <see cref="DrawBounds"/>.
    /// </summary>
    public Rect ComputedDrawBounds;

    /// <summary>
    /// The final rectangle that represents the rendered area of this control on the window.
    /// Do note that this rectangle may be obstructed by other rectangles, so this is not 
    /// representative of the currently visible area of this control, 
    /// only the area that is actively being rendered.
    /// </summary>
    public Rect Rendered;
}