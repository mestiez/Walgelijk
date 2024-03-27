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
    public bool DoubleClicked;

    public bool TabReleased;
    public bool ShiftHeld;
    public bool CtrlHeld;
    public bool EscapePressed;
    public bool DeletePressed;
    public bool BackspacePressed;

    public bool PastePressed;
    public bool CopyPressed;

    public bool EndPressed;
    public bool HomePressed;

    public string TextEntered = string.Empty;
    public readonly HashSet<Key> AlphanumericalHeld = new();

    public Vector2 DirectionKeyReleased;

    private Vector2 rawScrollDelta;
    private float lastClickTime;
    private Configuration Config => Onion.Configuration;

    public void Update(in InputState state, float dt)
    {
        MousePosition = state.WindowMousePosition;
        MouseDelta = state.WindowMouseDelta;

        MousePrimaryPressed = state.IsButtonPressed(MouseButton.Left);
        MousePrimaryHeld = state.IsButtonHeld(MouseButton.Left);
        MousePrimaryRelease = state.IsButtonReleased(MouseButton.Left);

        DoubleClicked = false;
        if (MousePrimaryRelease)
        {
            if (Onion.Clock - lastClickTime < Config.DoubleClickTimeWindow.TotalSeconds)
                DoubleClicked = true;
            else
                lastClickTime = Onion.Clock;
        }

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
        EndPressed = state.IsKeyPressed(Key.End);
        HomePressed = state.IsKeyPressed(Key.Home);
        BackspacePressed = state.IsKeyPressed(Key.Backspace);
        ShiftHeld = state.IsKeyHeld(Key.LeftShift);
        TabReleased = state.IsKeyReleased(Key.Tab);
        CtrlHeld = state.IsKeyHeld(Key.LeftControl);

        PastePressed = CtrlHeld && state.IsKeyPressed(Key.V);
        CopyPressed = CtrlHeld && state.IsKeyPressed(Key.C);

        AlphanumericalHeld.Clear();
        if (state.KeysHeld != null)
            foreach (var key in state.KeysHeld)
                if (((int)key >= (int)Key.A && (int)key <= (int)Key.Z) || ((int)key >= (int)Key.D0 && (int)key <= (int)Key.D9))
                    AlphanumericalHeld.Add(key);

        TextEntered = state.TextEntered;
    }
}
