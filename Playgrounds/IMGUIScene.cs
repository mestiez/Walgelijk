using System.Numerics;
using Walgelijk;
using Walgelijk.Imgui;

namespace TestWorld;

public struct IMGUIScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new GuiSystem());
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
            if (Gui.ClickButton("Hello World", new Vector2(32), new Vector2(128,32), HorizontalTextAlign.Center, VerticalTextAlign.Middle))
                Audio.PlayOnce(Sound.Beep);
        }

        public override void Render()
        {

        }
    }
}
