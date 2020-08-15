using System;
using System.Threading;
using Walgelijk;

namespace Walgelijk
{
    public class Game
    {
        private Scene scene;

        /// <summary>
        /// Currently active window
        /// </summary>
        public Window Window { get; }

        /// <summary>
        /// Currently active scene
        /// </summary>
        public Scene Scene
        {
            get => scene; set
            {
                scene = value;
                scene.Game = this;
            }
        }

        /// <summary>
        /// Returns the <see cref="Walgelijk.RenderQueue"/> that belongs to <see cref="Window"/>
        /// </summary>
        public RenderQueue RenderQueue => Window.RenderQueue;

        public Game(Window window)
        {
            Window = window;
            window.Game = this;
        }

        public void Start()
        {
            if (Window == null) throw new InvalidOperationException("Window is null");
            Window.StartLoop();
        }
    }
}
