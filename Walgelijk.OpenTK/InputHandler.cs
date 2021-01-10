using OpenTK;
using OpenTK.Input;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
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

            GameWindow.TextInput += TextEnter;
            GameWindow.KeyDown += KeyDown;
            GameWindow.KeyUp += KeyUp;

            GameWindow.MouseDown += MouseDown;
            GameWindow.MouseUp += MouseUp;

            GameWindow.MouseMove += MouseMove;
            GameWindow.MouseWheel += MouseWheel;

            inputState.TextEntered = "";
        }

        private void MouseWheel(MouseWheelEventArgs e)
        {
            inputState.MouseScrollDelta = GameWindow.MouseState.ScrollDelta.Y;
        }

        private void MouseMove(MouseMoveEventArgs e)
        {
            inputState.WindowMousePosition = new Vector2(e.X, e.Y);
            inputState.WindowMouseDelta = new Vector2(e.DeltaX, e.DeltaY);
        }

        private void MouseUp(MouseButtonEventArgs e)
        {
            inputState.MouseButtonsUp.Add(TypeConverter.Convert(e.Button));
            inputState.MouseButtonsHeld.Remove(TypeConverter.Convert(e.Button));
        }

        private void MouseDown(MouseButtonEventArgs e)
        {
            inputState.MouseButtonsDown.Add(TypeConverter.Convert(e.Button));
            inputState.MouseButtonsHeld.Add(TypeConverter.Convert(e.Button));
            inputState.AnyMouseButton = true;
        }

        private void KeyUp(KeyboardKeyEventArgs e)
        {
            inputState.KeysUp.Add(TypeConverter.Convert(e.Key));
            inputState.KeysHeld.Remove(TypeConverter.Convert(e.Key));
        }

        private void KeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace:
                    inputState.TextEntered += '\b';
                    break;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter:
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEnter:
                    inputState.TextEntered += '\n';
                    break;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab:
                    inputState.TextEntered += '\t';
                    break;
            }

            if (e.IsRepeat) return;

            inputState.KeysDown.Add(TypeConverter.Convert(e.Key));
            inputState.KeysHeld.Add(TypeConverter.Convert(e.Key));
            inputState.AnyKey = true;
        }

        private void TextEnter(TextInputEventArgs e)
        {
            inputState.TextEntered += e.AsString;
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
