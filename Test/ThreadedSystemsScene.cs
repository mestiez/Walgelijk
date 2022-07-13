using System;
using System.Numerics;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public struct ThreadedSystemsScene
{
    public static Scene Load(Game game)
    {
        var scene = new Scene(game);

        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        scene.AddSystem(new PressureSimulationSystem());
        scene.AddSystem(new ParallelSystemDiagramSystem());
        scene.AddSystem(new PressureRenderingSystem());

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#a8a3c1")
        });

        return scene;
    }

    public class PressureSimulationComponent
    {

    }

    [Parallel, RequiresComponents(typeof(PressureSimulationComponent))]
    public class PressureSimulationSystem : Walgelijk.System
    {

    }

    [Parallel, RequiresComponents(typeof(PressureSimulationComponent))]
    public class PressureRenderingSystem : Walgelijk.System
    {

    }

    public class ParallelSystemDiagramSystem : Walgelijk.System
    {
        public override void Render()
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.FontSize = 12;
            Draw.Font = Resources.Load<Font>("inter.fnt");

            int row = 4;
            int cursor = 0;
            const int height = 32;
            const int padding = 4;
            foreach (var group in Scene.GetParallelGroups())
            {
                cursor = 0;
                foreach (var sys in Scene.GetSystemsInParallelGroup(group))
                {
                    var n = sys.GetType().Name;
                    var width = (int)MathF.Ceiling(Draw.CalculateTextWidth(n) + 32);
                    var topLeft = new Vector2(cursor, row * (height + padding));
                    var size = new Vector2(width, height);

                    Draw.Colour = new Color(Utilities.Hash(row * 352.534f), Utilities.Hash(row * -52.4f), Utilities.Hash(row * 8.23f + 92.53f), 1);
                    Draw.Quad(topLeft, size);
                    Draw.Colour = Colors.White;
                    Draw.Text(n, topLeft + size / 2, Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Middle, width);
                    cursor += width + padding;
                }
                row++;
            }
        }
    }
}