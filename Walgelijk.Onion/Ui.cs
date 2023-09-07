using System.Runtime.CompilerServices;
using Walgelijk.Onion.Animations;
using Walgelijk.Onion.Assets;
using Walgelijk.Onion.Decorators;
using Walgelijk.Onion.Layout;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public static partial class Ui
{
    // The rest of this partial class is generated in Walgelijk.Onion.SourceGenerator
    // This is mainly where a few helpers and controls that are just other controls live

    public static bool IntStepper(ref int value, MinMax<int> range, int step = 1, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var imgCol = Ui.Theme.GetChanges().Image;

        const float arrowBoxSize = 16;
        float padding = Ui.Theme.Base.Padding;
        bool r = false;
        Ui.Decorators.KeepForNextTime(); 
        Ui.StartGroup(false, identity + 73, site); 
        {
            Ui.Layout.FitContainer(1, 1, pad: false).Scale(-arrowBoxSize + 1, 0);
            r |= Ui.IntInputBox(ref value, range, identity: identity + site);

            Ui.Layout.Width(arrowBoxSize).FitContainer(null, 0.5f, false).StickRight().Move(padding, 0);
            Ui.Theme.Image(imgCol).Once();
            if (Ui.HoldImageButton(BuiltInAssets.Icons.ChevronUp, ImageContainmentMode.Contain, identity: identity + site))
            {
                if (Onion.HoldTicker.IsTicked(Onion.Tree.GetLastInstance()))
                    value = Utilities.Clamp(value + step, range.Min, range.Max);
                r = true;
            }

            Ui.Layout.Width(arrowBoxSize).FitContainer(null, 0.5f, false).StickRight().StickBottom().Move(padding, padding);
            Ui.Theme.Image(imgCol).Once();
            if (Ui.HoldImageButton(BuiltInAssets.Icons.ChevronDown, ImageContainmentMode.Contain, identity: identity + site))
            {
                if (Onion.HoldTicker.IsTicked(Onion.Tree.GetLastInstance()))
                    value = Utilities.Clamp(value - step, range.Min, range.Max);
                r = true;
            }
        }
        Ui.End();

        return r;
    }

    public static bool FloatStepper(ref float value, MinMax<float> range, float step = 1, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var imgCol = Ui.Theme.GetChanges().Image; 
        const float arrowBoxSize = 16;
        float padding = Ui.Theme.Base.Padding;
        bool r = false;

        Ui.Decorators.KeepForNextTime();
        Ui.StartGroup(false, identity + 62, site);
        {
            Ui.Layout.FitContainer(1, 1, pad: false).Scale(-arrowBoxSize + 1, 0);
            r |= Ui.FloatInputBox(ref value, range, identity: identity + site);

            Ui.Layout.Width(arrowBoxSize).FitContainer(null, 0.5f, false).StickRight().Move(padding, 0);
            Ui.Theme.Image(imgCol).Once();
            if (Ui.HoldImageButton(BuiltInAssets.Icons.ChevronUp, ImageContainmentMode.Contain, identity: identity + site))
            {
                if (Onion.HoldTicker.IsTicked(Onion.Tree.GetLastInstance()))
                    value = Utilities.Clamp(value + step, range.Min, range.Max);
                r = true;
            }

            Ui.Layout.Width(arrowBoxSize).FitContainer(null, 0.5f, false).StickRight().StickBottom().Move(padding, padding);
            Ui.Theme.Image(imgCol).Once();
            if (Ui.HoldImageButton(BuiltInAssets.Icons.ChevronDown, ImageContainmentMode.Contain, identity: identity + site))
            {
                if (Onion.HoldTicker.IsTicked(Onion.Tree.GetLastInstance()))
                    value = Utilities.Clamp(value - step, range.Min, range.Max);
                r = true;
            }
        }
        Ui.End();

        return r;
    }

    public static void End() { Onion.Tree.End(); }

    /// <summary>
    /// Get the current control's state. This is the state of the current control, not the next.
    /// </summary>
    public static ControlState CurrentControlState
    {
        get
        {
            var node = Onion.Tree.CurrentNode ?? Onion.Tree.Root;
            var inst = Onion.Tree.EnsureInstance(node.Identity);
            return inst.State;
        }
    }

    /// <summary>
    /// Adds the specified animation to the animation queue.
    /// </summary>
    public static AnimationQueue Animate(in IAnimation anim) { Onion.Animation.Add(anim); return Onion.Animation; }

    /// <summary>
    /// Adds the specified decorator to the decorator queue.
    /// </summary>
    public static DecoratorQueue Decorate(in IDecorator dec) { Onion.Decorators.Add(dec); return Onion.Decorators; }

    /// <summary>
    /// Provides access to the layout queue.
    /// </summary>
    public static LayoutQueue Layout => Onion.Layout;

    /// <summary>
    /// Provides access to the animation queue.
    /// </summary>
    public static AnimationQueue Animation => Onion.Animation;

    /// <summary>
    /// Provides access to the decorator queue.
    /// </summary>
    public static DecoratorQueue Decorators => Onion.Decorators;

    /// <summary>
    /// Provides access to the theme stack.
    /// </summary>
    public static ThemeStack Theme => Onion.Theme;

    /// <summary>
    /// Returns true if any control is being interacted with (even hovereed over); false otherwise.
    /// See <see cref="Navigator.IsBeingUsed"/>
    /// </summary>
    public static bool IsBeingUsed => Onion.Navigator.IsBeingUsed;
    /// <summary>
    /// Returns true if any control is hovered over, see <see cref="Navigator.HoverControl"/>
    /// </summary>
    public static bool IsAnythingHover => Onion.Navigator.HoverControl.HasValue;
    /// <summary>
    /// Returns true if any control is active, see <see cref="Navigator.ActiveControl"/>
    /// </summary>
    public static bool IsAnythingActive => Onion.Navigator.ActiveControl.HasValue;
    /// <summary>
    /// Returns true if any control has focus, see <see cref="Navigator.FocusedControl"/>
    /// </summary>
    public static bool IsAnythingFocused => Onion.Navigator.FocusedControl.HasValue;
    /// <summary>
    /// Returns true if any control is triggered, see <see cref="Navigator.TriggeredControl"/>
    /// </summary>
    public static bool IsAnythingTriggered => Onion.Navigator.TriggeredControl.HasValue;

    /// <summary>
    /// Provides access to the global scale value.
    /// </summary>
    public static float GlobalScale
    {
        get => Onion.GlobalScale;
        set => Onion.GlobalScale = value;
    }
}
