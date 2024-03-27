using System.Numerics;
using Walgelijk.Onion.Animations;
using Walgelijk.Onion.Decorators;

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
    /// The name of this control. Sometimes used as a label.
    /// </summary>
    public string Name = "Untitled";

    /// <summary>
    /// The active rectangles that define the areas on the screen that represent this control for different purposes.
    /// </summary>
    public ControlRects Rects;

    /// <summary>
    /// Preferred width as determined by the control behaviour. Can be null.
    /// </summary>
    public float? PreferredWidth;

    /// <summary>
    /// Preferred height as determined by the control behaviour. Can be null.
    /// </summary>
    public float? PreferredHeight;

    /// <summary>
    /// The offset of every child as adjusted by scrolling
    /// </summary>
    public Vector2 InnerScrollOffset;

    /// <summary>
    /// Amount of seconds that this control will exist for even when no longer being called 
    /// (useful for exit animations)
    /// </summary>
    public float AllowedDeadTime = 0.3f;

    /// <summary>
    /// Returns true if <see cref="Navigator.FocusedControl"/> is <see cref="Identity"/>
    /// </summary>
    public bool HasFocus => Onion.Navigator.FocusedControl == Identity;

    /// <summary>
    /// Returns true if <see cref="Navigator.HoverControl"/> is <see cref="Identity"/>
    /// </summary>
    public bool IsHover => Onion.Navigator.HoverControl == Identity;

    /// <summary>
    /// Returns true if <see cref="Navigator.ActiveControl"/> is <see cref="Identity"/>
    /// </summary>
    public bool IsActive => Onion.Navigator.ActiveControl == Identity;

    /// <summary>
    /// Returns true if <see cref="Navigator.ScrollControl"/> is <see cref="Identity"/>
    /// </summary>
    public bool HasScroll => Onion.Navigator.ScrollControl == Identity;

    /// <summary>
    /// Returns true if <see cref="Navigator.TriggeredControl"/> is <see cref="Identity"/>
    /// </summary>
    public bool IsTriggered => Onion.Navigator.TriggeredControl == Identity;

    /// <summary>
    /// Returns true if <see cref="Navigator.KeyControl"/> is <see cref="Identity"/>
    /// </summary>
    public bool HasKeyboard => Onion.Navigator.KeyControl == Identity;

    /// <summary>
    /// The last unscaled timestamp when the control state (hover, active, etc.) was changed
    /// </summary>
    public float LastStateChangeTime = float.MinValue;

    /// <summary>
    /// Determines what events this control is capable of capturing. 
    /// Normally set to just <see cref="CaptureFlags.Hover"/>, it will only capture hover events.
    /// </summary>
    public CaptureFlags CaptureFlags = CaptureFlags.Hover;

    /// <summary>
    /// Should this control render the focus box if it has focus
    /// </summary>
    public bool RenderFocusBox = true;

    /// <summary>
    /// This control will not play any of the state change sounds if this set to true
    /// </summary>
    public bool Muted = false;

    /// <summary>
    /// Is this control newly created?
    /// </summary>
    public bool IsNew = true;

    /// <summary>
    /// List of visual animations to apply to this control
    /// </summary>
    public readonly AnimationAggregate Animations = new();

    /// <summary>
    /// Control-specific theme
    /// </summary>
    public Theme Theme;

    /// <summary>
    /// Determine the overflow behaviour, i.e what happens when the children don't fit inside this container
    /// </summary>
    public OverflowBehaviour OverflowBehaviour = OverflowBehaviour.ScrollHorizontal | OverflowBehaviour.ScrollVertical;

    /// <summary>
    /// List of decorators to apply to this control, maximum of 8
    /// </summary>
    public readonly ClampedArray<IDecorator> Decorators = new(DecoratorQueue.MaxDecoratorsPerControl);

    public ControlState State
    {
        get
        {
            var state = ControlState.None;

            if (Onion.Navigator.HoverControl == Identity)
                state |= ControlState.Hover;

            if (Onion.Navigator.ScrollControl == Identity)
                state |= ControlState.Scroll;

            if (Onion.Navigator.FocusedControl == Identity)
                state |= ControlState.Focus;

            if (Onion.Navigator.ActiveControl == Identity)
                state |= ControlState.Active;

            if (Onion.Navigator.KeyControl == Identity)
                state |= ControlState.Key;

            return state;
        }
    }

    public ControlInstance(int id)
    {
        Identity = id;
    }

    public float TimeSinceStateChange => Onion.Clock - LastStateChangeTime;

    public float GetTransitionProgress(float secondsSinceSceneChangeUnscaled) 
        => Utilities.Clamp((secondsSinceSceneChangeUnscaled - LastStateChangeTime) / Onion.Animation.DefaultDurationSeconds);

    public float GetTransitionProgress() 
        => Utilities.Clamp(TimeSinceStateChange / Onion.Animation.DefaultDurationSeconds);
}

[Flags]
public enum OverflowBehaviour
{
    ScrollHorizontal = 1,
    ScrollVertical = 2
}
