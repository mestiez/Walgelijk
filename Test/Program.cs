using System;
using System.Numerics;
using Walgelijk;
using Walgelijk.OpenTK;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(new OpenTKWindow("hallo daar", new Vector2(128, 128), new Vector2(800, 600)));
            game.Window.RenderTarget.ClearColour = new Color("0f3f00");
            game.Start();
        }
    }
}
