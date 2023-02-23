using System.Numerics;
using Walgelijk;
using Walgelijk.Imgui;
using Walgelijk.Onion;
using Walgelijk.Onion.Controls;
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
            //  Audio.PlayOnce(Sound.Beep);

            {
                Onion.Layout.Position(8, 8);
                Onion.Layout.Size(128, 32);
                Walgelijk.Onion.Controls.Button.Click("Hallo wereld!");
            }

            ////Onion.Tree.Start(23);
            ////Onion.Tree.Start(5);
            ////Onion.Tree.End();
            ////Onion.Tree.Start(635);
            ////Onion.Tree.End();
            ////Onion.Tree.Start(6235);
            ////Onion.Tree.End();
            ////Onion.Tree.End();

            //Onion.Layout.Position(8, 64);
            //Onion.Layout.Size(1024, 1024);
            //Onion.Tree.Start(51, new ScrollView());
            //if (Input.IsKeyHeld(Key.K))
            //{
            //    Onion.Layout.Position(0, 0);
            //    Onion.Layout.Size(128, 32);
            //    Walgelijk.Onion.Controls.Button.Click("Hallo wereld!");
            //}

            //if (!Input.IsKeyHeld(Key.L))
            //{
            //    Onion.Layout.Position(0, 64);
            //    Onion.Layout.Size(200, 100);
            //    if (Input.IsKeyHeld(Key.S))
            //        Onion.Layout.Constraints.Enqueue(new FitContainer(1, null));
            //    if (Walgelijk.Onion.Controls.Button.Click("Ik besta ook"))
            //        Audio.PlayOnce(Sound.Beep); 
            //}
            //Onion.Tree.End();

            if (!Input.IsKeyHeld(Key.L))
            {
                Onion.Layout.Size(128, Window.Height / 2);
                Onion.Layout.Position(Window.Width / 2, 64);
                Onion.Tree.Start(75, new ScrollView());
                for (int i = 0; i < 12; i++)
                {
                    Onion.Layout.Position(0, i * (32 + 8));
                    Onion.Layout.Size(128, 32);
                    if (Walgelijk.Onion.Controls.Button.Click("Ik besta ook", i))
                        Audio.PlayOnce(Sound.Beep);
                }
                Onion.Tree.End();
            }

            //Onion.Tree.Start(535);
            //Onion.Tree.End();

            //Onion.Tree.Start(42);
            //Onion.Tree.End();
        }

        public override void Render()
        {

        }
    }
}
