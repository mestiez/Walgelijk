using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// A game window
/// </summary>
public abstract class Window
{
    /// <summary>
    /// Title of the window
    /// </summary>
    public abstract string Title { get; set; }
    /// <summary>
    /// Position of the window on the display in pixels
    /// </summary>
    public abstract Vector2 Position { get; set; }
    /// <summary>
    /// Size of the window in pixels
    /// </summary>
    public abstract Vector2 Size { get; set; }
    /// <summary>
    /// Identical to (int)Size.X
    /// </summary>
    public int Width => (int)Size.X;
    /// <summary>
    /// Identical to (int)Size.Y
    /// </summary>
    public int Height => (int)Size.Y;
    /// <summary>
    /// Should vertical synchronisation be enabled
    /// </summary>
    public abstract bool VSync { get; set; }
    /// <summary>
    /// Is the window open?
    /// </summary>
    public abstract bool IsOpen { get; }
    /// <summary>
    /// Does the window have user focus?
    /// </summary>
    public abstract bool HasFocus { get; }
    /// <summary>
    /// Is the window visible?
    /// </summary>
    public abstract bool IsVisible { get; set; }
    /// <summary>
    /// Is the window resizable?
    /// </summary>
    public abstract bool Resizable { get; set; }   
    /// <summary>
    /// Determines window border type
    /// </summary>
    public abstract WindowType WindowType { get; set; }
    /// <summary>
    /// Graphics functions
    /// </summary>
    public abstract IGraphics Graphics { get; }

    public abstract bool IsCursorLocked { get; set; }

    /// <summary>
    /// The window render queue. It stores the render tasks and is emptied and executed every render frame.
    /// </summary>
    public RenderQueue RenderQueue { get; } = new RenderQueue();

    /// <summary>
    /// The <see cref="Walgelijk.Game"/> this window originates from
    /// </summary>
    public Game Game { get; set; }

    /// <summary>
    /// The main rendertarget for this window
    /// </summary>
    public abstract RenderTarget RenderTarget { get; }

    /// <summary>
    /// Fires when the window is resized. Provides new size
    /// </summary>
    public event EventHandler<Vector2>? OnResize;
    /// <summary>
    /// Fires when the window is moved. Provides new position
    /// </summary>
    public event EventHandler<Vector2>? OnMove;
    /// <summary>
    /// Fires when a file is dropped on the window, Provides file paths
    /// </summary>
    public event EventHandler<string[]>? OnFileDrop;
    /// <summary>
    /// Fires when the window is going to close
    /// </summary>
    public event EventHandler? OnClose;

    /// <summary>
    /// Called by the game before the main loop starts
    /// </summary>
    public abstract void Initialise();

    /// <summary>
    /// Called by the game when the game loop has ended
    /// </summary>
    public abstract void Deinitialise();

    /// <summary>
    /// Called every frame by the game
    /// </summary>
    public abstract void LoopCycle();

    /// <summary>
    /// Close the window and stop the game loop
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// Turn screen coordinates into window coordinates
    /// </summary>
    /// <returns></returns>
    public abstract Vector2 ScreenToWindowPoint(Vector2 screen);
    /// <summary>
    /// Turn window coordinates into screen coordinates
    /// </summary>
    /// <returns></returns>
    public abstract Vector2 WindowToScreenPoint(Vector2 window);
    /// <summary>
    /// Turn world coordinates into window coordinates
    /// </summary>
    /// <returns></returns>
    public abstract Vector2 WorldToWindowPoint(Vector2 world);
    /// <summary>
    /// Turn window coordinates into world coordinates
    /// </summary>
    /// <returns></returns>
    public abstract Vector2 WindowToWorldPoint(Vector2 window);

    /// <summary>
    /// Invoke the resize event
    /// </summary>
    /// <param name="newSize"></param>
    protected void InvokeResizeEvent(Vector2 newSize) => OnResize?.Invoke(this, newSize);

    /// <summary>
    /// Invoke the move event
    /// </summary>
    /// <param name="newPosition"></param>
    protected void InvokeMoveEvent(Vector2 newPosition) => OnMove?.Invoke(this, newPosition);

    /// <summary>
    /// Invoke the file drop event
    /// </summary>
    /// <param name="path"></param>
    protected void InvokeFileDropEvent(string[] path) => OnFileDrop?.Invoke(this, path);

    /// <summary>
    /// Invoke the close event
    /// </summary>
    protected void InvokeCloseEvent() => OnClose?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Manually reset the input state
    /// </summary>
    public abstract void ResetInputState();

    /// <summary>
    /// Set the window icon
    /// </summary>
    public abstract void SetIcon(IReadableTexture texture, bool flipY = true);
}
