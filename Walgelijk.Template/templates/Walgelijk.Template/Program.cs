using System.Numerics;
using Walgelijk;
using Walgelijk.OpenTK;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Template;

public class Program
{
    public static Game Game = null!;

    // Program entry point
    public static void Main(string[] args)
    {
        // Create the "Game" instance. This object keeps track of literally everything in your game.
        Game = new Game(
            new OpenTKWindow("Walgelijk.Template", new Vector2(-1), new Vector2(1280, 720)),
            new OpenALAudioRenderer()
        );

        // Set variable update rate cap to 120. This means that the Update and Render calls will occur at no more than 120 Hz.
        // You can set this to 0 to uncap framerate, but that might make your GPU whine.
        Game.UpdateRate = 120;
        // Set the fixed update rate to 60. This means that the FixedUpdate calls will occur at 60 Hz, no matter the framerate.
        Game.FixedUpdateRate = 60;

        Game.Window.VSync = false;

        // When a texture is loaded using the Resources system, it is 
        TextureLoader.Settings.FilterMode = FilterMode.Linear;

        // The resource system is easier to use if you set base paths for different resource types.
        Resources.SetBasePathForType<FixedAudioData>("audio");
        Resources.SetBasePathForType<StreamAudioData>("audio");
        Resources.SetBasePathForType<Texture>("textures");
        Resources.SetBasePathForType<Font>("fonts");

        // Here we create a scene and add some systems to it. Scenes contain entities, components, and systems.
        // Entities are the binding between different components, components contain data, systems manipulate components.
        var scene = Game.Scene = new Scene(Game);
        scene.AddSystem(new CameraSystem());
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new ExampleSystem());

        // Create a camera and add it to the scene
        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent()); // Cameras need transform components, they have information about position, scale, and rotation
        scene.AttachComponent(camera, new CameraComponent() // Cameras also need the camera component. The have information about zoom, clear colour, etc.
            {
                PixelsPerUnit = 1, // One pixel per in-game unit. For example, an object being at (10, 0, 0) means it's 10 "units" from the world origin. Pixels per unit determines how many pixels represent a single unit.
                OrthographicSize = 1, // The "zoom" of the camera. This is independent from PixelsPerUnit, but it essentially does the same thing. The larger this value, the further it is zoomed out.
                ClearColour = new Color("#2c2436") // The background colour of the world
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

        // Set the game window icon using Resources.Load<Texture>, which will load a texture using the config in TextureLoader.Settings
        Game.Window.SetIcon(Resources.Load<Texture>("icon.png"));

        // The quick profiler shows some performance stats :)
        Game.Profiling.DrawQuickProfiler = false;

        Game.Start();
    }

    // Example component for the example scene
    public class ExampleComponent : Walgelijk.Component
    {
        public Vector2 Position;
        public float Speed = 5;
    }

    // Example system for the example scene
    public class ExampleSystem : Walgelijk.System
    {
        public override void FixedUpdate()
        {
            foreach (var inst in Scene.GetAllComponentsOfType<ExampleComponent>())
            {
                // Query for user input
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
            // Always good practice to reset the SimpleDraw state before 
            Draw.Reset();
            foreach (var inst in Scene.GetAllComponentsOfType<ExampleComponent>())
            {
                //Draw image for each instance of ExampleComponent
                Draw.Image(
                    Texture.ErrorTexture, // We will just use the error texture for now
                    new Rect(inst.Position, new Vector2(48)), // Create a rectangle at center inst.Position with a size of (48,48)
                    ImageContainmentMode.Stretch // Stretch the image over rectangle
                    );
            }
        }
    }
}
