using System.Numerics;

namespace Walgelijk.Onion;

public class Input
{
    public Vector2 ScrollDelta;
    public Vector2 MousePosition;

    public bool MousePrimaryPressed;
    public bool MousePrimaryHeld;
    public bool MousePrimaryRelease;

    private Vector2 rawScrollDelta;
    private Configuration Config => Onion.Configuration;

    public void Update(in InputState state, float dt)
    {
        MousePosition = state.WindowMousePosition;

        MousePrimaryPressed = state.IsButtonPressed(Button.Left);
        MousePrimaryHeld = state.IsButtonHeld(Button.Left);
        MousePrimaryRelease = state.IsButtonReleased(Button.Left);

        rawScrollDelta = Vector2.Zero;
        if (state.IsKeyHeld(Config.ScrollHorizontal))
            rawScrollDelta.X -= state.MouseScrollDelta;
        else
            rawScrollDelta.Y += state.MouseScrollDelta;

        if (Config.SmoothScroll > float.Epsilon)
        {
            float speed = 24 / Config.SmoothScroll;
            var delta = (rawScrollDelta * Config.ScrollSensitivity) / Config.SmoothScroll ;
            ScrollDelta += delta; //TODO vogel uit hoe die deltatime en scroll speed gedoe
            ScrollDelta = Utilities.SmoothApproach(ScrollDelta, Vector2.Zero, speed, dt);
        }
        else
            ScrollDelta = rawScrollDelta * Config.ScrollSensitivity;
    }
}
