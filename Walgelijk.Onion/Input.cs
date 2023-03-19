using System.Numerics;

namespace Walgelijk.Onion;

public class Input
{
    public Vector2 ScrollDelta;
    public Vector2 MousePosition;
    public Vector2 MouseDelta;

    public bool MousePrimaryPressed;
    public bool MousePrimaryHeld;
    public bool MousePrimaryRelease;

    public bool TabReleased;
    public bool ShiftHeld;
    public bool CtrlHeld;
    public bool EscapePressed;
    public bool DeletePressed;
    public bool BackspacePressed;

    public string TextEntered = string.Empty;
    public readonly HashSet<Key> AlphanumericalHeld = new();

    public Vector2 DirectionKeyReleased;
    private Vector2 rawScrollDelta;
    private Configuration Config => Onion.Configuration;

    public void Update(in InputState state, float dt)
    {
        MousePosition = state.WindowMousePosition;
        MouseDelta = state.WindowMouseDelta;

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
            var delta = (rawScrollDelta * Config.ScrollSensitivity) / Config.SmoothScroll;
            ScrollDelta += delta; //TODO vogel uit hoe die deltatime en scroll speed gedoe
            ScrollDelta = Utilities.SmoothApproach(ScrollDelta, Vector2.Zero, speed, dt);
        }
        else
            ScrollDelta = rawScrollDelta * Config.ScrollSensitivity;

        DirectionKeyReleased = default;

        if (state.IsKeyReleased(Key.Right))
            DirectionKeyReleased.X += 1;

        if (state.IsKeyReleased(Key.Left))
            DirectionKeyReleased.X -= 1;

        if (state.IsKeyReleased(Key.Up))
            DirectionKeyReleased.Y += 1;

        if (state.IsKeyReleased(Key.Down))
            DirectionKeyReleased.Y -= 1;

        EscapePressed = state.IsKeyPressed(Key.Escape);
        DeletePressed = state.IsKeyPressed(Key.Delete);
        BackspacePressed = state.IsKeyPressed(Key.Backspace);
        ShiftHeld = state.IsKeyHeld(Key.LeftShift);
        TabReleased = state.IsKeyReleased(Key.Tab);
        CtrlHeld = state.IsKeyHeld(Key.LeftControl);

        AlphanumericalHeld.Clear();
        if (state.KeysHeld != null)
            foreach (var key in state.KeysHeld)
                if (((int)key >= (int)Key.A && (int)key <= (int)Key.Z) || ((int)key >= (int)Key.D0 && (int)key <= (int)Key.D9))
                    AlphanumericalHeld.Add(key);

        TextEntered = state.TextEntered;
    }
}
