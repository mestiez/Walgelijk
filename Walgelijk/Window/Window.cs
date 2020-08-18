using System;
using System.Numerics;
using Walgelijk;

namespace Walgelijk
{
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
        /// Speed the game should render at
        /// </summary>
        public abstract int TargetFrameRate { get; set; }
        /// <summary>
        /// Speed the game should run updates at
        /// </summary>
        public abstract int TargetUpdateRate { get; set; }
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
        /// The current input state
        /// </summary>
        public abstract InputState InputState { get; }

        /// <summary>
        /// Time information
        /// </summary>
        public abstract Time Time { get; }

        /// <summary>
        /// The window render queue. It stores the render tasks and is emptied and executed every render frame.
        /// </summary>
        public RenderQueue RenderQueue { get; } = new RenderQueue();

        /// <summary>
        /// The <see cref="Walgelijk.Game"/> this window originates from
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// RenderTarget for this window
        /// </summary>
        public abstract RenderTarget RenderTarget { get; }

        /// <summary>
        /// Provides shader specific functions
        /// </summary>
        public abstract IShaderManager ShaderManager { get; }

        /// <summary>
        /// Fires when the window is resized. Provides new size
        /// </summary>
        public event EventHandler<Vector2> OnResize;
        /// <summary>
        /// Fires when the window is moved. Provides new position
        /// </summary>
        public event EventHandler<Vector2> OnMove;
        /// <summary>
        /// Fires when a file is dropped on the window, Provides file paths
        /// </summary>
        public event EventHandler<string[]> OnFileDrop;
        /// <summary>
        /// Fires when the window is going to close
        /// </summary>
        public event EventHandler OnClose;

        /// <summary>
        /// Start the main game loop
        /// </summary>
        public abstract void StartLoop();

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
    }
}
