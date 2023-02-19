using System.Numerics;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public class OnionSystem : Walgelijk.System
{
    public bool DebugDrawTree = true;

    public override void Initialise()
    {
    }

    public override void Update()
    {
        Onion.Tree.End();
        Onion.Tree.Process(Time.DeltaTime);
        Onion.Layout.Position(0, 0, Layout.Space.Absolute);
        Onion.Layout.Size(Window.Width, Window.Height);
        Onion.Tree.Start(0, null); //Root node
    }

    public override void Render()
    {
        Onion.Tree.Render();

        if (DebugDrawTree)
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = RenderOrder.DebugUI;

            var offset = new Vector2(64, 64);
            draw(Onion.Tree.Root);

            void draw(Node node)
            {
                Draw.Colour = Colors.Gray;
                if (node.AliveLastFrame)
                    Draw.Colour = Colors.Yellow;
                if (node.Alive)
                    Draw.Colour = Colors.Green;

                Draw.Text(node.ToString() ?? "[untitled]", offset, Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Bottom);
                offset.X += 32;
                foreach (var item in node.Children)
                {
                    offset.Y += 16;
                    draw(item.Value);
                }
                offset.X -= 32;
            }
        }
    }
}
