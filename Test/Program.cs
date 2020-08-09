using System;
using System.Numerics;
using Walgelijk;
using Walgelijk.SilkImplementation;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(new SilkWindow("hallo daar", new Vector2(128, 128), new Vector2(800, 600)));
            game.Start();
        }
    }
}
