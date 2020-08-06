using System;
using System.Collections.Generic;
using System.Numerics;

namespace WalgelijkCore
{
    public class Walgelijk
    {
        public Window Window { get; }

        public Walgelijk(Window window)
        {
            Window = window;
        }

        public void Start()
        {

        }
    }

    public abstract class Window
    {
        public abstract Vector2 Position { get; set; }
        public abstract Vector<int> Size { get; set; }
        public abstract bool IsOpen { get; }
        public abstract bool HasFocus { get; }

        public Window()
        {

        }
    }
}
