using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{
    public class InputHandler
    {
        private InputState inputState = new();

        public InputHandler(OpenTKWindow window)
        {
            Window = window;

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
            inputState.MouseScrollDelta = /*GameWindow.MouseState.ScrollDelta.Y */e.OffsetY;
        }

        private void MouseMove(MouseMoveEventArgs e)
        {
            var n = new Vector2(e.X, e.Y);
            inputState.WindowMouseDelta = n - inputState.WindowMousePosition;
            inputState.WindowMousePosition = n;
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
            inputState.MouseScrollDelta = 0;// GameWindow.MouseState.ScrollDelta.Y;

            var n = Window.WindowToWorldPoint(inputState.WindowMousePosition);
            inputState.WorldMouseDelta = n - inputState.WorldMousePosition;
            inputState.WorldMousePosition = n;
        }

        public OpenTKWindow Window { get; }
        public NativeWindow GameWindow => Window.window;
        public InputState InputState { get => inputState; }
    }
}
