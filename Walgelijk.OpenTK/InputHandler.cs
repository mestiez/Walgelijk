using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{
    public class InputHandler
    {
        private InputState inputState;

        public InputHandler(OpenTKWindow window)
        {
            Window = window;

            inputState.MouseButtonsDown = new HashSet<int>();
            inputState.MouseButtonsHeld = new HashSet<int>();
            inputState.MouseButtonsUp = new HashSet<int>();

            inputState.KeysDown = new HashSet<int>();
            inputState.KeysHeld = new HashSet<int>();
            inputState.KeysUp = new HashSet<int>();

            GameWindow.KeyPress += TextEnter;
            GameWindow.KeyDown += KeyDown;
            GameWindow.KeyUp += KeyUp;

            GameWindow.MouseDown += MouseDown;
            GameWindow.MouseUp += MouseUp;

            GameWindow.MouseMove += MouseMove;
            GameWindow.MouseWheel += MouseWheel;
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            inputState.MouseScrollDelta = e.DeltaPrecise;
        }

        private void MouseMove(object sender, MouseMoveEventArgs e)
        {
            inputState.WindowMousePosition = new Vector2(e.X, e.Y);
            inputState.WindowMouseDelta = new Vector2(e.XDelta, e.YDelta);
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            inputState.MouseButtonsUp.Add((int)e.Button);
            inputState.MouseButtonsHeld.Remove((int)e.Button);
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            inputState.MouseButtonsDown.Add((int)e.Button);
            inputState.MouseButtonsHeld.Add((int)e.Button);
            inputState.AnyMouseButton = true;
        }

        private void KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            inputState.KeysUp.Add((int)e.Key);
            inputState.KeysHeld.Remove((int)e.Key);
        }

        private void KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            inputState.KeysDown.Add((int)e.Key);
            inputState.KeysHeld.Add((int)e.Key);
            inputState.AnyKey = true;
        }

        private void TextEnter(object sender, KeyPressEventArgs e)
        {
            inputState.TextEntered += e.KeyChar;
        }

        public void Reset()
        {
            inputState.KeysDown.Clear();
            inputState.KeysUp.Clear();
            inputState.MouseButtonsDown.Clear();
            inputState.MouseButtonsUp.Clear();

            inputState.TextEntered = "";
            inputState.AnyMouseButton = false;
            inputState.AnyKey = false;
            inputState.MouseScrollDelta = 0;
            inputState.WindowMouseDelta = Vector2.Zero;
        }

        public OpenTKWindow Window { get; }
        public GameWindow GameWindow => Window.window;
        public InputState InputState { get => inputState; }
    }
}
