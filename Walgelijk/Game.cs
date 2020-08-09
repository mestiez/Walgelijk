using System;
using System.Threading;

namespace Walgelijk
{
    public class Game
    {
        public Window Window { get; }

        public Game(Window window)
        {
            Window = window;
        }

        public void Start()
        {
            if (Window == null) throw new InvalidOperationException("Window is null");
            Window.StartLoop();
        }
    }
}
