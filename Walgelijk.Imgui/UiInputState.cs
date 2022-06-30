using System;
using System.Numerics;

namespace Walgelijk.Imgui
{
    public class UiInputState
    {
        public bool[] ButtonsDown = global::System.Array.Empty<bool>();
        public (bool prev, bool current)[] ButtonsHeld = global::System.Array.Empty<(bool, bool)>();
        public bool[] ButtonsUp = global::System.Array.Empty<bool>();

        public bool[] KeysDown = global::System.Array.Empty<bool>();
        public (bool prev, bool current)[] KeysHeld = global::System.Array.Empty<(bool, bool)>();
        public bool[] KeysUp = global::System.Array.Empty<bool>();

        public Vector2 LastWindowMousePos;
        public Vector2 WindowMousePos;
        public Vector2 WindowMousePosDelta;
        public string TextEntered = string.Empty;
        public float ScrollDelta;

        public bool IsButtonPressed(Button b) => ButtonsDown[(int)b];
        public bool IsButtonHeld(Button b) => ButtonsHeld[(int)b].current;
        public bool IsButtonReleased(Button b) => ButtonsUp[(int)b];

        public bool IsKeyPressed(Key b) => KeysDown[(int)b];
        public bool IsKeyHeld(Key b) => KeysHeld[(int)b].current;
        public bool IsKeyReleased(Key b) => KeysUp[(int)b];

        public virtual void Clear()
        {
            Array.Fill(ButtonsDown, false);
            Array.Fill(ButtonsUp, false);
            Array.Fill(ButtonsHeld, (false, false));

            Array.Fill(KeysDown, false);
            Array.Fill(KeysUp, false);
            Array.Fill(KeysHeld, (false, false));

            TextEntered = string.Empty;
            ScrollDelta = 0;
        }
    }

    public class ControlInputState : UiInputState
    {
        public bool IsMouseOver;
        public bool HasScrollFocus;

        public override void Clear()
        {
            base.Clear();
            IsMouseOver = false;
            HasScrollFocus = false;
        }
    }
}