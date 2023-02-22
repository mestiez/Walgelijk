using System.Numerics;
using Walgelijk.Onion.Layout;

namespace Walgelijk.Onion;

public static class Onion
{
    public static readonly LayoutState Layout = new();
    public static readonly ControlTree Tree = new();
    public static readonly Navigator Navigator = new();
    public static readonly Input Input = new();
    public static readonly Configuration Configuration = new();

    /*TODO 
     * control state (hot, active, capture, scroll, etc.)
     * raycast
     * scissorbox (drawarea? GetDrawBox() of iets. )
     * ClearEverything();
     * style
     *      style moet textures meer supporten, niet alleen kleuren 
     *      misschien zelfs iets anders dan quads
     *      uber shader voor alle controls
     * Stack<Style> en dan bouw je voor elke control een final style misschien?
     * heel veel basic functies hier (label, button. etc.)
     * Animation system (IAnimation) deel van style? nee toch??? weet ik het 
     * fix die node deletion shit ControlTree.cs:93
     * navigation (arrows, tab)
     *      tab: cycle through all controls chronologically
     *      arrows: move from control to control based on position in space
    */
}

public class Input
{
    public Vector2 ScrollDelta;
    public Vector2 MousePosition;

    private Vector2 rawScrollDelta;
    private Configuration Config => Onion.Configuration;

    public void Update(in InputState state, float dt)
    {
        MousePosition = state.WindowMousePosition;

        rawScrollDelta = Vector2.Zero;
        if (state.IsKeyHeld(Config.ScrollHorizontal))
            rawScrollDelta.X -= state.MouseScrollDelta;
        else
            rawScrollDelta.Y += state.MouseScrollDelta;

        Config.SmoothScroll = 1;

        if (Config.SmoothScroll > float.Epsilon)
        {
            float speed = 24 / Config.SmoothScroll;
            ScrollDelta += rawScrollDelta / Config.SmoothScroll;
            ScrollDelta = Utilities.SmoothApproach(ScrollDelta, Vector2.Zero, speed, dt);
        }
        else
            ScrollDelta = rawScrollDelta * Config.ScrollSensitivity;
    }
}

public class Configuration
{
    public int RenderLayer = RenderOrder.UI.Layer;
    public float ScrollSensitivity = 24;
    public Key ScrollHorizontal = Key.LeftShift;
    public float SmoothScroll = 1;
}