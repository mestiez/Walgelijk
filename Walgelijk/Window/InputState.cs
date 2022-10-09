using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Struct that simply holds input data
/// </summary>
public struct InputState
{
    /// <summary>
    /// All mouse buttons that have been pressed last frame
    /// </summary>
    public readonly HashSet<Button>? MouseButtonsDown = new();
    /// <summary>
    /// All mouse buttons that are currently held
    /// </summary>
    public readonly HashSet<Button>? MouseButtonsHeld = new();
    /// <summary>
    /// All moues buttons that have been released last frame
    /// </summary>
    public readonly HashSet<Button>? MouseButtonsUp = new();
    /// <summary>
    /// All keys that have been pressed last frame
    /// </summary>
    public readonly HashSet<Key>? KeysDown = new();
    /// <summary>
    /// All keys that are currently held
    /// </summary>
    public readonly HashSet<Key>? KeysHeld = new();
    /// <summary>
    /// All keys that were released last frame
    /// </summary>
    public readonly HashSet<Key>? KeysUp = new();

    /// <summary>
    /// Mouse position in window coordinates
    /// </summary>
    public Vector2 WindowMousePosition = default;
    /// <summary>
    /// Mouse position in world coordinates
    /// </summary>
    public Vector2 WorldMousePosition = default;
    /// <summary>
    /// Mouse movement delta over 1 frame in window coordinates
    /// </summary>
    public Vector2 WindowMouseDelta = default;
    /// <summary>
    /// Mouse movement delta over 1 frame in world coordinates
    /// </summary>
    public Vector2 WorldMouseDelta = default;
    /// <summary>
    /// Mouse wheel delta over 1 frame
    /// </summary>
    public float MouseScrollDelta = default;

    /// <summary>
    /// Returns if any key is down
    /// </summary>
    public bool AnyKey = default;
    /// <summary>
    /// Returns if any mouse button is down
    /// </summary>
    public bool AnyMouseButton = default;

    /// <summary>
    /// The text entered last frame
    /// </summary>
    public string TextEntered = string.Empty;

    public InputState()
    {
    }

    #region helper methods
    /// <summary>
    /// Returns if the key is held
    /// </summary>
    public bool IsKeyHeld(Key key) => KeysHeld?.Contains(key) ?? false;
    /// <summary>
    /// Retusn if the key was pressed
    /// </summary>
    public bool IsKeyPressed(Key key) => KeysDown?.Contains(key) ?? false;
    /// <summary>
    /// Returns if the key was released
    /// </summary>
    public bool IsKeyReleased(Key key) => KeysUp?.Contains(key) ?? false;
    /// <summary>
    /// Returns if the key is held
    /// </summary>
    public bool IsButtonHeld(Button button) => MouseButtonsHeld?.Contains(button) ?? false;
    /// <summary>
    /// Retusn if the key was pressed
    /// </summary>
    public bool IsButtonPressed(Button button) => MouseButtonsDown?.Contains(button) ?? false;
    /// <summary>
    /// Returns if the key was released
    /// </summary>
    public bool IsButtonReleased(Button button) => MouseButtonsUp?.Contains(button) ?? false;
    #endregion

    /// <summary>
    /// Reset the given <see cref="InputState"/>
    /// </summary>
    public static void Reset(ref InputState inputState)
    {
        inputState.KeysDown?.Clear();
        inputState.KeysUp?.Clear();
        inputState.MouseButtonsDown?.Clear();
        inputState.MouseButtonsUp?.Clear();

        inputState.TextEntered = "";
        inputState.AnyMouseButton = false;
        inputState.AnyKey = false;

        inputState.WindowMouseDelta = Vector2.Zero;
    }
}
