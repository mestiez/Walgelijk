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
        Ui.StartGroup(false, identity, site);
        {
            Ui.Layout.FitContainer(1, 1, pad: false).Scale(-arrowBoxSize + 1, 0);
            r |= Ui.IntInputBox(ref value, range, identity: identity);

            Ui.Layout.Width(arrowBoxSize).FitContainer(null, 0.5f, false).StickRight().Move(padding, 0);
            Ui.Theme.Image(imgCol).Once();
            if (Ui.HoldImageButton(BuiltInAssets.Icons.ChevronUp, ImageContainmentMode.Contain))
            {
                if (Onion.HoldTicker.IsTicked(Onion.Tree.GetLastInstance()))
                    value = Utilities.Clamp(value + step, range.Min, range.Max);
                r = true;
            }

            Ui.Layout.Width(arrowBoxSize).FitContainer(null, 0.5f, false).StickRight().StickBottom().Move(padding, padding);
            Ui.Theme.Image(imgCol).Once();
            if (Ui.HoldImageButton(BuiltInAssets.Icons.ChevronDown, ImageContainmentMode.Contain))
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

    public static AnimationQueue Animate(in IAnimation anim) { Onion.Animation.Add(anim); return Onion.Animation; }
    public static DecoratorQueue Decorate(in IDecorator dec) { Onion.Decorators.Add(dec); return Onion.Decorators; }

    public static LayoutQueue Layout => Onion.Layout;
    public static AnimationQueue Animation => Onion.Animation;
    public static DecoratorQueue Decorators => Onion.Decorators;
    public static ThemeStack Theme => Onion.Theme;
}