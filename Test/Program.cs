using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Walgelijk;
using Walgelijk.NAudio;
using Walgelijk.OpenTK;
using Walgelijk.ParticleSystem;

namespace Test
{
    class Program
    {
        private static Game game;

        static void Main(string[] args)
        {
            game = new Game(
                new OpenTKWindow("hallo daar", new Vector2(-1, -1), new Vector2(800, 600)),
                new NAudioRenderer()
                );

            game.Window.TargetFrameRate = 0;
            game.Window.TargetUpdateRate = 0;
            game.Window.VSync = false;

            Resources.SetBasePathForType<Sound>("audio");
            Resources.SetBasePathForType<Prefab>("prefabs");
            Resources.SetBasePathForType<Texture>("textures");
            Resources.SetBasePathForType<IReadableTexture>("textures");
            Resources.SetBasePathForType<IWritableTexture>("textures");
            Resources.SetBasePathForType<Font>("fonts");

            game.Scene = LoadScene(game);

            game.Start();
        }

        private static Scene LoadScene(Game game)
        {
            Scene scene = new Scene(game);

            var camera = scene.CreateEntity();
            scene.AttachComponent(camera, new TransformComponent());
            scene.AttachComponent(camera, new CameraComponent { PixelsPerUnit = 1, OrthographicSize = 0.02f });

            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new CameraSystem());
            scene.AddSystem(new ShapeRendererSystem());
            scene.AddSystem(new WaveMovementSystem());
            scene.AddSystem(new DebugCameraSystem());
            scene.AddSystem(new ParticleSystem());


            var particles = scene.CreateEntity();
            scene.AttachComponent(particles, new TransformComponent());
            scene.AttachComponent(particles, new WaveMovementComponent());
            scene.AttachComponent(particles, new ParticlesComponent(1000)
            {
               // Dampening = new FloatRange(0.9f),
                RotationalDampening = new FloatRange(0.95f),
               // Gravity = new Vec2Range(Vector2.Zero),
                StartVelocity = new Vec2Range(Vector2.One * -15, Vector2.One * 15),
                EmissionRate = 640,
                StartColor = new ColorRange(Colors.White),
                ColorOverLife = new ColorCurve(new Curve<Color>.Key(Color.White, 0), new Curve<Color>.Key(Color.Red, 1)),
                SizeOverLife = new FloatCurve(new Curve<float>.Key(0, 0), new Curve<float>.Key(1, 0.1f), new Curve<float>.Key(0, 1)),
                WorldSpace = true,
                SimulationSpeed = 2f
            });

            return scene;
        }
    }

    [RequiresComponents(typeof(TransformComponent))]
    public class WaveMovementComponent
    {
        public float Amplitude = 25;
        public float Frequency = 2;
        public float Phase;
    }

    public class WaveMovementSystem : Walgelijk.System
    {
        public override void Update()
        {
            var components = Scene.GetAllComponentsOfType<WaveMovementComponent>();
            foreach (var item in components)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(item.Entity);
                var wave = item.Component;

                transform.Position = new Vector2(MathF.Sin(Time.SecondsSinceLoad * wave.Frequency + wave.Phase) * wave.Amplitude, 0);
            }
        }
    }
}
