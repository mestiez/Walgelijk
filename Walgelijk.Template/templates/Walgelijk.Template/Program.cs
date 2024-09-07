using System.Numerics;
using Walgelijk;
using Walgelijk.OpenTK;
using Walgelijk.SimpleDrawing;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;
using Walgelijk.CommonAssetDeserialisers;
using Walgelijk.CommonAssetDeserialisers.Audio;
using Walgelijk.CommonAssetDeserialisers.Audio.Qoa;

namespace Walgelijk.Template;

public class Program
{
    public static Game Game = null!;

    public static void Main(string[] args)
    {
        Game = new Game(
            new OpenTKWindow("Walgelijk.Template", new Vector2(-1), new Vector2(1280, 720)),
            new OpenALAudioRenderer()
        );

        Game.UpdateRate = 120;
        Game.FixedUpdateRate = 60;
        Game.Window.VSync = false;

        TextureLoader.Settings.FilterMode = FilterMode.Linear;

        AssetDeserialisers.Register(new OggFixedAudioDeserialiser());
        AssetDeserialisers.Register(new OggStreamAudioDeserialiser());
        AssetDeserialisers.Register(new QoaFixedAudioDeserialiser());
        AssetDeserialisers.Register(new WaveFixedAudioDeserialiser());
        AssetDeserialisers.Register(new FontDeserialiser());

        foreach (var a in Directory.EnumerateFiles("assets", "*.waa"))
            Assets.RegisterPackage(a);

        var scene = Game.Scene = new Scene(Game);
        scene.AddSystem(new CameraSystem());
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new ExampleSystem());

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent()); 
        scene.AttachComponent(camera, new CameraComponent() 
        {
            PixelsPerUnit = 1, 
            OrthographicSize = 1, 
            ClearColour = new Color(0xff0061)
        }
        );

        // Create our example thing
        var example = scene.CreateEntity();
        scene.AttachComponent(example, new ExampleComponent());

#if DEBUG
        Game.DevelopmentMode = true;
        Game.Console.DrawConsoleNotification = true;
#else
        Game.DevelopmentMode = false;
        Game.Console.DrawConsoleNotification = false;
#endif

        Game.Window.SetIcon(Assets.LoadNoCache<Texture>("textures/icon.png"));
        Game.Profiling.DrawQuickProfiler = false;
        Game.Compositor.Enabled = false;

        Game.Start();
    }
}

public class ExampleComponent : Walgelijk.Component
{
    public Vector2 Position;
    public float Speed = 5;
}

public class ExampleSystem : Walgelijk.System
{
    public override void FixedUpdate()
    {
        foreach (var inst in Scene.GetAllComponentsOfType<ExampleComponent>())
        {
            if (Input.IsKeyHeld(Key.A))
                inst.Position.X -= inst.Speed;
            if (Input.IsKeyHeld(Key.D))
                inst.Position.X += inst.Speed;
            if (Input.IsKeyHeld(Key.W))
                inst.Position.Y += inst.Speed;
            if (Input.IsKeyHeld(Key.S))
                inst.Position.Y -= inst.Speed;
        }
    }

    public override void Render()
    {
        Draw.Reset();
        Draw.Font = Assets.Load<Font>("fonts/inter.wf");
        Draw.Colour = Colors.White;
        Draw.FontSize = 72;
        foreach (var inst in Scene.GetAllComponentsOfType<ExampleComponent>())
        {
            Draw.Text("Het werkt gewoon", inst.Position, Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Middle);
        }
    }
}