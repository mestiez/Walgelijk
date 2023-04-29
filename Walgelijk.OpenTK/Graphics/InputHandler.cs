using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{
    public class InputHandler
    {
        public InputState InputState = new();

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

            InputState.TextEntered = "";
        }

        private void MouseWheel(MouseWheelEventArgs e)
        {
            InputState.MouseScrollDelta = /*GameWindow.MouseState.ScrollDelta.Y */e.OffsetY;
        }

        private void MouseMove(MouseMoveEventArgs e)
        {
            var n = new Vector2(e.X, e.Y);
            InputState.WindowMouseDelta = n - InputState.WindowMousePosition;
            InputState.WindowMousePosition = n;
        }

        private void MouseUp(MouseButtonEventArgs e)
        {
            InputState.MouseButtonsUp.Add(TypeConverter.Convert(e.Button));
            InputState.MouseButtonsHeld.Remove(TypeConverter.Convert(e.Button));
        }

        private void MouseDown(MouseButtonEventArgs e)
        {
            InputState.MouseButtonsDown.Add(TypeConverter.Convert(e.Button));
            InputState.MouseButtonsHeld.Add(TypeConverter.Convert(e.Button));
            InputState.AnyMouseButton = true;
        }

        private void KeyUp(KeyboardKeyEventArgs e)
        {
            InputState.KeysUp.Add(TypeConverter.Convert(e.Key));
            InputState.KeysHeld.Remove(TypeConverter.Convert(e.Key));
        }

        private void KeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace:
                    InputState.TextEntered += '\b';
                    break;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter:
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEnter:
                    InputState.TextEntered += '\n';
                    break;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab:
                    InputState.TextEntered += '\t';
                    break;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete:
                    InputState.TextEntered += '\u007F';
                    break;         
            }

            if (e.IsRepeat) return;

            InputState.KeysDown.Add(TypeConverter.Convert(e.Key));
            InputState.KeysHeld.Add(TypeConverter.Convert(e.Key));
            InputState.AnyKey = true;
        }

        private void TextEnter(TextInputEventArgs e)
        {
            InputState.TextEntered += e.AsString;
        }

        public void Reset()
        {
            InputState.Reset(ref InputState);
            InputState.MouseScrollDelta = 0;// GameWindow.MouseState.ScrollDelta.Y;

            var n = Window.WindowToWorldPoint(InputState.WindowMousePosition);
            InputState.WorldMouseDelta = n - InputState.WorldMousePosition;
            InputState.WorldMousePosition = n;
        }

        public OpenTKWindow Window { get; }
        public NativeWindow GameWindow => Window.window;
    }
}
