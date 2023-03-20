using System.Reflection;
using System.Runtime.CompilerServices;
using Walgelijk.Onion.Assets;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion;

public static class Onion
{
    public static readonly Layout.Layout Layout = new();
    public static readonly ControlTree Tree = new();
    public static readonly Navigator Navigator = new();
    public static readonly Input Input = new();
    public static readonly Configuration Configuration = new();
    public static readonly AnimationQueue Animation = new();
    public static Theme Theme = new();
    public static float SoundVolume = 0.5f;
    public static AudioTrack? AudioTrack;

    public static readonly Material ControlMaterial = OnionMaterial.CreateNew();

    static Onion()
    {
        CommandProcessor.RegisterAssembly(Assembly.GetAssembly(typeof(Onion)) ?? throw new Exception("I do not exist."));
    }

    [Command(Alias = "OnionClear", HelpString = "Clears the Onion UI cache, effectively resetting the UI scene")]
    public static void ClearCache()
    {
        Layout.Reset();
        Tree.Clear();
        Navigator.Clear();
    }

    #region Controls

    //public static ControlState Text(string text, HorizontalTextAlign horizontal, VerticalTextAlign vertical, int identity = 0, [CallerLineNumber] int site = 0)
    //    => TextRect.Create(text, horizontal, vertical, identity, site);

    #endregion

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
                Game.Main.AudioRenderer.PlayOnce(sound, SoundVolume, 1, AudioTrack);
        }
    }

    /*TODO 
     * scrollbars etc. (pseudo controls)
     * heel veel basic functies hier (label, button. etc.)
    */
}

public class Theme
{
    public Appearance Background = new Color("#022525");
    public Appearance Foreground = new Color("#055555");
    public Color Text = new Color("#fcffff");
    public Color Accent = new Color("#de3a67");
    public Color Highlight = new Color("#ffffff");

    public Font Font = Walgelijk.Font.Default;
    public int FontSize = 12;

    public float Padding = 5;
    public float Rounding = 1;
    public float WindowTitleBarHeight = 24;

    public Color FocusBoxColour = new Color("#3adeda");
    public float FocusBoxSize = 5;
    public float FocusBoxWidth = 4;

    public Sound? HoverSound;
    public Sound? ActiveSound = new(BuiltInAssets.Click, false, false);
    public Sound? ScrollSound;
    public Sound? TriggerSound;
    public Sound? FocusSound;
}

public class ThemeProperty<T> where T : notnull
{
    public readonly T Default;

    public ThemeProperty(in T @default)
    {
        Default = @default;
    }

    private readonly Stack<T> stack = new();

    public void Push(T val) => stack.Push(val);

    public T Pop() => stack.Pop();

    public T Get()
    {
        if (stack.TryPeek(out var val))
            return val;
        return Default;
    }

    public static implicit operator T(ThemeProperty<T> theme) => theme.Get();
}
