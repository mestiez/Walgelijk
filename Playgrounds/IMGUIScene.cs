using System.Numerics;
using Walgelijk;
using Walgelijk.Imgui;
using Walgelijk.Onion;
using Walgelijk.Onion.Layout;

namespace TestWorld;

public struct IMGUIScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new GuiSystem());
        scene.AddSystem(new OnionSystem());
        scene.AddSystem(new IMGUITestSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#272830")
        });
        game.UpdateRate = 120;
        return scene;
    }

    public class IMGUITestSystem : Walgelijk.System
    {
        public override void Initialise()
        {

        }

        public override void Update()
        {
            //if (Gui.ClickButton("Hello World", new Vector2(32), new Vector2(128, 32), HorizontalTextAlign.Center, VerticalTextAlign.Middle))
            //    Audio.PlayOnce(Sound.Beep);

            //Onion.Tree.Start(23, null);
            //Onion.Tree.Start(5, null);
            //Onion.Tree.End();
            //Onion.Tree.Start(635, null);
            //Onion.Tree.End();
            //Onion.Tree.Start(6235, null);
            //Onion.Tree.End();
            //Onion.Tree.End();

            Onion.Tree.Start(51, null);
            if (Input.IsKeyHeld(Key.K))
            {
                Onion.Tree.Start(425, null);
                Onion.Layout.Position(8, 8);
                Onion.Layout.Size(128, 32);
                Walgelijk.Onion.Controls.Button.Click("Hallo wereld!");
                Onion.Tree.End();
            }
            Onion.Tree.End();

            //if (!Input.IsKeyHeld(Key.L))
            //{
            //    Onion.Layout.Position(0, 256);
            //    Onion.Layout.Size(200, 100);
            //    if (Input.IsKeyHeld(Key.S))
            //        Onion.Layout.SelfLayout.Enqueue(new FitContainer(1, null));
            //    Walgelijk.Onion.Controls.Button.Click("Hallo wereld!");
            //}

            //Onion.Tree.Start(75, null);
            //Onion.Tree.End();

            //Onion.Tree.Start(535, null);
            //Onion.Tree.End();

            //Onion.Tree.Start(42, null);
            //Onion.Tree.End();
        }

        public override void Render()
        {

        }
    }
}
