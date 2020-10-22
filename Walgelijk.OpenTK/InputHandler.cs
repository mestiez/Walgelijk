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

            inputState.MouseButtonsDown = new HashSet<Button>();
            inputState.MouseButtonsHeld = new HashSet<Button>();
            inputState.MouseButtonsUp = new HashSet<Button>();

            inputState.KeysDown = new HashSet<Key>();
            inputState.KeysHeld = new HashSet<Key>();
            inputState.KeysUp = new HashSet<Key>();

            GameWindow.KeyPress += TextEnter;
            GameWindow.KeyDown += KeyDown;
            GameWindow.KeyUp += KeyUp;

            GameWindow.MouseDown += MouseDown;
            GameWindow.MouseUp += MouseUp;

            GameWindow.MouseMove += MouseMove;
            GameWindow.MouseWheel += MouseWheel;

            inputState.TextEntered = "";
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
            inputState.MouseButtonsUp.Add(TypeConverter.Convert(e.Button));
            inputState.MouseButtonsHeld.Remove(TypeConverter.Convert(e.Button));
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            inputState.MouseButtonsDown.Add(TypeConverter.Convert(e.Button));
            inputState.MouseButtonsHeld.Add(TypeConverter.Convert(e.Button));
            inputState.AnyMouseButton = true;
        }

        private void KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            inputState.KeysUp.Add(TypeConverter.Convert(e.Key));
            inputState.KeysHeld.Remove(TypeConverter.Convert(e.Key));
        }

        private void KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case global::OpenTK.Input.Key.BackSpace:
                    inputState.TextEntered += '\b';
                    break;
                case global::OpenTK.Input.Key.Enter:
                case global::OpenTK.Input.Key.KeypadEnter:
                    inputState.TextEntered += '\n';
                    break;
                case global::OpenTK.Input.Key.Tab:
                    inputState.TextEntered += '\t';
                    break;
            }

            if (e.IsRepeat) return;

            inputState.KeysDown.Add(TypeConverter.Convert(e.Key));
            inputState.KeysHeld.Add(TypeConverter.Convert(e.Key));
            inputState.AnyKey = true;
        }

        private void TextEnter(object sender, KeyPressEventArgs e)
        {
            //if (Keyboard.GetState().IsKeyDown(global::OpenTK.Input.Key.BackSpace))
            //    inputState.TextEntered += '\b';
            //else
                inputState.TextEntered += e.KeyChar;
        }

        public void Reset()
        {
            InputState.Reset(ref inputState);
            inputState.WorldMousePosition = Window.WindowToWorldPoint(inputState.WindowMousePosition);
        }

        public OpenTKWindow Window { get; }
        public GameWindow GameWindow => Window.window;
        public InputState InputState { get => inputState; }
    }
}
