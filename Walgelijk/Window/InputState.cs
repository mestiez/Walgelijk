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
        public HashSet<int> MouseButtonsDown;
        /// <summary>
        /// All mouse buttons that are currently held
        /// </summary>
        public HashSet<int> MouseButtonsHeld;
        /// <summary>
        /// All moues buttons that have been released last frame
        /// </summary>
        public HashSet<int> MouseButtonsUp;
        /// <summary>
        /// All keys that have been pressed last frame
        /// </summary>
        public HashSet<int> KeysDown;
        /// <summary>
        /// All keys that are currently held
        /// </summary>
        public HashSet<int> KeysHeld;
        /// <summary>
        /// All keys that were released last frame
        /// </summary>
        public HashSet<int> KeysUp;

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

        /// <summary>
        /// Returns if the key is held
        /// </summary>
        public bool IsKeyHeld(int key) => KeysHeld.Contains(key);
    }
}
