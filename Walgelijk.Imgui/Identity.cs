using System.Numerics;

namespace Walgelijk.Imgui;

public delegate Vector2 OffsetLayout(Identity parent, Identity current, Vector2 currentPosition, Style? style = null);
public delegate float WidthLayout(Identity parent, Identity current, float currentWidth, Style? style = null);
public delegate float HeightLayout(Identity parent, Identity current, float currentHeight, Style? style = null);

public delegate float HorizontalAnchor(Identity parent, Identity current, float currentX, Style? style = null);
public delegate float VerticalAnchor(Identity parent, Identity current, float currentY, Style? style = null);
public delegate float WidthAnchor(Identity parent, Identity current, float currentWidth, Style? style = null);
public delegate float HeightAnchor(Identity parent, Identity current, float currentHeight, Style? style = null);

public class Identity
{
    /// <summary>
    /// Did this control exist in the last frame?
    /// </summary>
    public bool ExistedLastFrame;
    /// <summary>
    /// Is this control present in the scene?
    /// </summary>
    public bool Exists;

    /// <summary>
    /// The integer ID of this control
    /// </summary>
    public int Raw;

    /// <summary>
    /// The integer ID of this control's parent, if applicable
    /// </summary>
    public int? Parent;

    /// <summary>
    /// Amount of children. This is usually not accurate because the amount changes as controls are added.
    /// </summary>
    public int ChildCount;

    /// <summary>
    /// Amount of children in the previous frame. This is the final counted amount from last frame and it's usually what you're looking for.
    /// </summary>
    public int PreviousChildCount;

    /// <summary>
    /// Combined size of all children. <b>This is not the inner content bounds of the control!</b>
    /// </summary>
    public Vector2 ChildSizeSum;

    /// <summary>
    /// Min and max edges of every child in local space
    /// </summary>
    public Rect InnerContentBounds;

#if false
    /// <summary>
    /// The calculated scroll bounds. (lowerX, upperX, lowerY, upperY)
    /// </summary>
    public (float lowerX, float upperX, float lowerY, float upperY) CalculatedScrollBounds;
#endif

    /// <summary>
    /// How much to offset all children
    /// </summary>
    public Vector2 InnerScrollOffset;

    /// <summary>
    /// Does the <see cref="InnerContentBounds"/> extend beyond the control bounds? If this returns true, the control should probably become a scrollable surface.
    /// </summary>
    public bool ChildrenExtendOutOfBounds
    {
        get
        {
            if (InnerContentBounds.MinX < TopLeft.X)
                return true;
            if (InnerContentBounds.MinY < TopLeft.Y)
                return true;
            if (InnerContentBounds.MaxX > TopLeft.X + Size.X)
                return true;
            if (InnerContentBounds.MaxY > TopLeft.Y + Size.Y)
                return true;
            return false;
        }
    }

    /// <summary>
    /// This control's position is not affected by the layout system or draw bounds system.
    /// </summary>
    public bool AbsoluteLayout;

    /// <summary>
    /// This control's width is not affected by the layout system.
    /// </summary>
    public bool AbsoluteWidth;

    /// <summary>
    /// This control's width is not affected by the layout system.
    /// </summary>
    public bool AbsoluteHeight;

    /// <summary>
    /// Absolute translation offset
    /// </summary>
    public Vector2 AbsoluteTranslation;

    /// <summary>
    /// nth child of my parent
    /// </summary>
    public int SiblingIndex;

    /// <summary>
    /// Current layout cursor position, if applicable
    /// </summary>
    public float LayoutCursorPosition;

    /// <summary>
    /// The maximum layout cursor position, if applicable.
    /// </summary>
    public float MaxLayoutCursorPosition;

    /// <summary>
    /// The top left of the control
    /// </summary>
    public Vector2 TopLeft;

    /// <summary>
    /// The size of the control
    /// </summary>
    public Vector2 Size;

    /// <summary>
    /// The order considered when raycasting for input
    /// </summary>
    public int Order;

    /// <summary>
    /// Raycast target rectangle
    /// </summary>
    public Rect? RaycastRectangle;

    /// <summary>
    /// The bounds in which this control and its children are drawn
    /// </summary>
    public DrawBounds DrawBounds;

    /// <summary>
    /// The offset layout delegate. Determines positioning of children. Can be null.
    /// </summary>
    public OffsetLayout? OffsetLayout;

    /// <summary>
    /// The width layout delegate. Determines width of children. Can be null.
    /// </summary>
    public WidthLayout? WidthLayout;

    /// <summary>
    /// The height layout delegate. Determines height of children. Can be null.
    /// </summary>
    public HeightLayout? HeightLayout;

    /// <summary>
    /// Is true if the control is requesting scroll input
    /// </summary>
    public bool WantsToEatScrollInput;

    /// <summary>
    /// The input state local to this control
    /// </summary>
    public ControlInputState LocalInputState = new();

    /// <summary>
    /// Animation state
    /// </summary>
    public AnimationState AnimationState;

    /// <summary>
    /// Raw value as string
    /// </summary>
    /// <returns></returns>
    public override string? ToString() => Raw.ToString();
}

/// <summary>
/// Stores time data for controls
/// </summary>
public struct AnimationState
{
    /// <summary>
    /// Seconds this control has existed for
    /// </summary>
    public float TimeAlive;

    /// <summary>
    /// Seconds this control has been hot
    /// </summary>
    public float TimeHot;

    /// <summary>
    /// Seconds this control has been active
    /// </summary>
    public float TimeActive;
}