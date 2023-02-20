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
        // end previous frame
        Onion.Tree.End();

        // process
        Onion.Tree.Process(Time.DeltaTime);
        Onion.Navigator.Process(Input);

        // next frame
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
            float h = 0.5f;

            var offset = new Vector2(64, 64);
            draw(Onion.Tree.Root);

            void draw(Node node)
            {
                Draw.Colour = Colors.Gray;
                if (node.AliveLastFrame)
                    Draw.Colour = Colors.Yellow;
                if (node.Alive)
                    Draw.Colour = Color.FromHsv(h, 0.6f, 1);

                Draw.Text((node.ToString() ?? "[untitled]") + " D: " + node.ComputedGlobalOrder, offset, Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Bottom);
                offset.X += 32;
                h += 0.15f;
                foreach (var item in node.Children)
                {
                    offset.Y += 16;
                    draw(item.Value);
                }
                h -= 0.15f;
                offset.X -= 32;
            }
        }
    }
}
