using Walgelijk;
using Walgelijk.Imgui;

namespace Videogame.Scenes;

public struct MainScene
{
    public static Scene Create(Game game)
    {
        var scene = new Scene(game);

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            Clear = true,
            ClearColour = Colors.Black,
            OrthographicSize = 1,
            PixelsPerUnit = 128,
        });

        scene.AddSystem(new DebugCameraSystem());
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new CameraSystem());
        scene.AddSystem(new GuiSystem());

        return scene;
    }
}