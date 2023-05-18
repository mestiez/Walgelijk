using Walgelijk.Onion.Animations;
using Walgelijk.Onion.Decorators;
using Walgelijk.Onion.Layout;

namespace Walgelijk.Onion;

public static partial class Ui
{
    // The rest of this partial class is generated in Walgelijk.Onion.SourceGenerator

    public static void End() { Onion.Tree.End(); }

    public static AnimationQueue Animate(in IAnimation anim) { Onion.Animation.Add(anim); return Onion.Animation; }
    public static DecoratorQueue Decorate(in IDecorator dec) { Onion.Decorators.Add(dec); return Onion.Decorators; }

    public static LayoutQueue Layout => Onion.Layout;
    public static AnimationQueue Animation => Onion.Animation;
    public static DecoratorQueue Decorators => Onion.Decorators;
    public static ThemeStack Theme => Onion.Theme;
}