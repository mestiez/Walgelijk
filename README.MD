![logo](https://imagedelivery.net/5o26WuCU9khNJnleuT61wg/a3029739-7d73-4b13-2a2e-6088af77ee00/public)

![Nuget](https://img.shields.io/nuget/v/Walgelijk?color=1650EC&logo=nuget&style=flat-square)

Een kleine, simpele 2D game engine

```cs
var game = new Game(
	new OpenTKWindow("Videogame", new Vector2(-1), new Vector2(1280, 720)),
	new OpenALAudioRenderer()
);

game.UpdateRate = 120;
game.FixedUpdateRate = 60;
game.Window.VSync = false;

TextureLoader.Settings.FilterMode = FilterMode.Linear;

Resources.SetBasePathForType<FixedAudioData>("audio");
Resources.SetBasePathForType<StreamAudioData>("audio");
Resources.SetBasePathForType<Texture>("textures");
Resources.SetBasePathForType<Font>("fonts");

var scene = new Scene(game);
game.Scene = scene;
scene.AddSystem(new CameraSystem());
scene.AddSystem(new TransformSystem());
		
var camera = scene.CreateEntity();
scene.AttachComponent(camera, new TransformComponent());
scene.AttachComponent(camera, new CameraComponent
{
    PixelsPerUnit = 1,
    OrthographicSize = 1,
    ClearColour = new Color("#a8a3c1")
});

#if DEBUG
	game.DevelopmentMode = true;
	game.Console.DrawConsoleNotification = true;
#else
	game.DevelopmentMode = false;
	game.Console.DrawConsoleNotification = false;
#endif

game.Window.SetIcon(Resources.Load<Texture>("icon.png"));
game.Profiling.DrawQuickProfiler = false;

game.Start();
```
**De [resources](/Walgelijk/resources) folder moet worden gekopieerd wanneer je een nieuw project maakt omdat daar de standaard shaders en fonts in zitten**
