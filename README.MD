![logo](https://i.imgur.com/YMGhfSS.png)

Een kleine, simpele 2D game engine

```cs
Game game = new Game(
    new OpenTKWindow("Titel", default, new Vector2(1280,720)), 
    new NAudioRenderer()
    );

game.Window.TargetFrameRate = 120;
game.Window.TargetUpdateRate = 60;
game.Window.VSync = false;
game.Window.RenderTarget.ClearColour = new Color("#000000");

var scene = new Scene();

var camera = scene.CreateEntity();
scene.AttachComponent(camera, new CameraComponent());
scene.AttachComponent(camera, new TransformComponent());

scene.AddSystem(new TransformSystem());
scene.AddSystem(new CameraSystem());

game.Scene = scene;
game.Start();
```
