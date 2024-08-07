﻿using System.Reflection;
using Walgelijk.Onion.Animations;
using Walgelijk.Onion.Assets;
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
    public static readonly ThemeStack Theme = new();

    //public static readonly PseudoControls PseudoControls = new();

    public static Sound? HoverSound;
    public static Sound? ActiveSound = new(BuiltInAssets.Click, false, null);
    public static Sound? ScrollSound;
    public static Sound? TriggerSound;
    public static Sound? FocusSound;

    public static float GlobalScale = 1;
    public static readonly HoldTicker HoldTicker = new();

    public static bool Initialised { get; private set; }

    public static float Clock = 0;

    public static readonly Hook OnClear = new();
    internal static bool ForceClearCache = false;

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
        Theme.Reset();

        OnClear.Dispatch();
    }

    [Command(Alias = "OnionScale", HelpString = "Sets or gets the global Onion UI scale")]
    public static CommandResult SetScale(float scale = float.NaN)
    {
        if (float.IsNaN(scale))
            return $"Global scale is {GlobalScale}";
        if (scale <= float.Epsilon)
            return CommandResult.Error("Scale has to be greater than 0");
        GlobalScale = scale;
        return $"Global scale set to {GlobalScale}";
    }

    public static void PlaySound(ControlState state)
    {
        switch (state)
        {
            case ControlState.Hover:
                p(HoverSound);
                break;
            case ControlState.Scroll:
                p(ScrollSound);
                break;
            case ControlState.Focus:
                p(FocusSound);
                break;
            case ControlState.Active:
                p(ActiveSound);
                break;
            case ControlState.Triggered:
                p(TriggerSound);
                break;
        }

        static void p(Sound? sound)
        {
            if (sound != null)
                Game.Main.AudioRenderer.PlayOnce(sound, Configuration.SoundVolume, 1, Configuration.AudioTrack);
        }
    }
}

//public class PseudoControls
//{
//    public delegate void ScrollbarDrawing(in ControlParams p, in Rect background, in Rect tracker);
//    public ScrollbarDrawing? DrawScrollbar;
//}