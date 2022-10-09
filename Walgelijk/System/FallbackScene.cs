namespace Walgelijk;

internal struct FallbackScene
{
    private static Scene? fallbackScene;

    public static Scene GetFallbackScene(Game game)
    {
        if (fallbackScene != null)
            return fallbackScene;

        fallbackScene = new Scene(game);

        var camera = fallbackScene.CreateEntity();
        fallbackScene.AttachComponent(camera, new TransformComponent());
        fallbackScene.AttachComponent(camera, new CameraComponent
        {
            Clear = true,
            ClearColour = Colors.Black,
            OrthographicSize = 1,
            PixelsPerUnit = 128,
        });

        var text = fallbackScene.CreateEntity();
        fallbackScene.AttachComponent(text, new TransformComponent());
        fallbackScene.AttachComponent(text, new TextComponent("No scene assigned"));

        fallbackScene.AddSystem(new TransformSystem());
        fallbackScene.AddSystem(new ShapeRendererSystem());
        fallbackScene.AddSystem(new CameraSystem());

        return fallbackScene;
    }
}
