using System.Reflection;
using Walgelijk.Onion.Animations;
using Walgelijk.Onion.Decorators;
using Walgelijk.Onion.Layout;

namespace Walgelijk.Onion;

public static class Onion
{
    public static readonly LayoutQueue Layout = new();
    public static readonly ControlTree Tree = new();
    public static readonly Navigator Navigator = new();
    public static readonly Input Input = new();
    public static readonly Configuration Configuration = new();

    public static readonly AnimationQueue Animation = new();
    public static readonly DecoratorQueue Decorators = new();

    public static bool Initialised { get; private set; }
    public static readonly Hook OnClear = new();

    static Onion()
    {
        CommandProcessor.RegisterAssembly(Assembly.GetAssembly(typeof(Onion)) ?? throw new Exception("I do not exist."));
    }

    internal static void Initalise(Game game)
    {
        if (Initialised)
            return;

        Initialised = true;
        game.OnSceneChange.AddListener(_ => ClearCache());
    }

    [Command(Alias = "OnionClear", HelpString = "Clears the Onion UI cache, effectively resetting the UI scene")]
    public static void ClearCache()
    {
        Tree.Clear();
        Animation.Clear();
        Layout.Reset();
        Navigator.Clear();
        Decorators.Clear();

        OnClear.Dispatch();
    }

    public static void PlaySound(ControlState state)
    {
        switch (state)
        {
            case ControlState.Hover:
                p(Theme.HoverSound);
                break;
            case ControlState.Scroll:
                p(Theme.ScrollSound);
                break;
            case ControlState.Focus:
                p(Theme.FocusSound);
                break;
            case ControlState.Active:
                p(Theme.ActiveSound);
                break;
            case ControlState.Triggered:
                p(Theme.TriggerSound);
                break;
        }

        static void p(Sound? sound)
        {
            if (sound != null)
                Game.Main.AudioRenderer.PlayOnce(sound, Configuration.SoundVolume, 1, Configuration.AudioTrack);
        }
    }

    #region Controls

    //public static ControlState Text(string text, HorizontalTextAlign horizontal, VerticalTextAlign vertical, int identity = 0, [CallerLineNumber] int site = 0)
    //    => TextRect.Create(text, horizontal, vertical, identity, site);

    #endregion

    /*TODO 
     * scrollbars etc. (pseudo controls)
     * heel veel basic functies hier (label, button. etc.)
    */
}
