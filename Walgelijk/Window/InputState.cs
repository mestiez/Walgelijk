using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Struct that simply holds input data
    /// </summary>
    public struct InputState
    {
        /// <summary>
        /// All mouse buttons that have been pressed last frame
        /// </summary>
        public HashSet<Button> MouseButtonsDown;
        /// <summary>
        /// All mouse buttons that are currently held
        /// </summary>
        public HashSet<Button> MouseButtonsHeld;
        /// <summary>
        /// All moues buttons that have been released last frame
        /// </summary>
        public HashSet<Button> MouseButtonsUp;
        /// <summary>
        /// All keys that have been pressed last frame
        /// </summary>
        public HashSet<Key> KeysDown;
        /// <summary>
        /// All keys that are currently held
        /// </summary>
        public HashSet<Key> KeysHeld;
        /// <summary>
        /// All keys that were released last frame
        /// </summary>
        public HashSet<Key> KeysUp;

        /// <summary>
        /// Mouse position in window coordinates
        /// </summary>
        public Vector2 WindowMousePosition;
        /// <summary>
        /// Mouse position in world coordinates
        /// </summary>
        public Vector2 WorldMousePosition;
        /// <summary>
        /// Mouse movement delta over 1 frame in window coordinates
        /// </summary>
        public Vector2 WindowMouseDelta;
        /// <summary>
        /// Mouse movement delta over 1 frame in world coordinates
        /// </summary>
        public Vector2 WorldMouseDelta;
        /// <summary>
        /// Mouse wheel delta over 1 frame
        /// </summary>
        public float MouseScrollDelta;

        /// <summary>
        /// Returns if any key is down
        /// </summary>
        public bool AnyKey;
        /// <summary>
        /// Returns if any mouse button is down
        /// </summary>
        public bool AnyMouseButton;

        /// <summary>
        /// The text entered last frame
        /// </summary>
        public string TextEntered;

        #region helper methods
        /// <summary>
        /// Returns if the key is held
        /// </summary>
        public bool IsKeyHeld(Key key) => KeysHeld.Contains(key);
        /// <summary>
        /// Retusn if the key was pressed
        /// </summary>
        public bool IsKeyPressed(Key key) => KeysDown.Contains(key);
        /// <summary>
        /// Returns if the key was released
        /// </summary>
        public bool IsKeyReleased(Key key) => KeysUp.Contains(key);
        /// <summary>
        /// Returns if the key is held
        /// </summary>
        public bool IsButtonHeld(Button button) => MouseButtonsHeld.Contains(button);
        /// <summary>
        /// Retusn if the key was pressed
        /// </summary>
        public bool IsButtonPressed(Button button) => MouseButtonsDown.Contains(button);
        /// <summary>
        /// Returns if the key was released
        /// </summary>
        public bool IsButtonReleased(Button button) => MouseButtonsUp.Contains(button);
        #endregion

        /// <summary>
        /// Reset the given <see cref="InputState"/>
        /// </summary>
        public static void Reset(ref InputState inputState)
        {
            inputState.KeysDown.Clear();
            inputState.KeysUp.Clear();
            inputState.MouseButtonsDown.Clear();
            inputState.MouseButtonsUp.Clear();

            inputState.TextEntered = "";
            inputState.AnyMouseButton = false;
            inputState.AnyKey = false;

            inputState.WindowMouseDelta = Vector2.Zero;
        }
    }
}
