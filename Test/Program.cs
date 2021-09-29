using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Walgelijk;
using Walgelijk.OpenTK;
using Walgelijk.ParticleSystem;
using Walgelijk.UI;

namespace Test
{
    class Program
    {
        private static Game game;

        static void Main(string[] args)
        {
            game = new Game(
                new OpenTKWindow("hallo daar", new Vector2(-1, -1), new Vector2(800, 600)),
                new OpenALAudioRenderer()
                );

            game.Window.TargetFrameRate = 0;
            game.Window.TargetUpdateRate = 0;
            game.Console.DrawConsoleNotification = true;
            game.Window.VSync = false;

            Resources.SetBasePathForType<AudioData>("audio");
            Resources.SetBasePathForType<Prefab>("prefabs");
            Resources.SetBasePathForType<Texture>("textures");
            Resources.SetBasePathForType<Font>("fonts");

            //game.Scene = SplashScreen.CreateScene(new[]{
            //    new SplashScreen.Logo(Resources.Load<Texture>("walgelijk.png"), 0.5f),
            //    },
            //() =>
            //{
            //});
            game.Scene = LoadScene(game);

#if DEBUG
            game.DevelopmentMode = true;
#else
            game.DevelopmentMode = false;
#endif
            game.Window.SetIcon(Resources.Load<Texture>("icon.png"));

            game.Start();
        }

        private static Scene LoadScene(Game game)
        {
            Scene scene = new Scene(game);

            RenderTexture gaming = new RenderTexture(128, 128);
            game.Window.Graphics.CurrentTarget = gaming;
            game.Window.Graphics.Clear(Colors.Purple);
            game.Window.Graphics.Draw(PrimitiveMeshes.Circle, Material.DefaultTextured);

            var camera = scene.CreateEntity();
            scene.AttachComponent(camera, new TransformComponent());
            scene.AttachComponent(camera, new CameraComponent { PixelsPerUnit = 1, OrthographicSize = 0.02f });

            scene.AttachComponent(scene.CreateEntity(), new UiDataComponent());

            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
            scene.AddSystem(new ShapeRendererSystem());
            scene.AddSystem(new WaveMovementSystem());
            scene.AddSystem(new DebugCameraSystem());
            scene.AddSystem(new ParticleSystem());
            scene.AddSystem(new ElementTransformSystem());
            scene.AddSystem(new ElementEventSystem());
            scene.AddSystem(new ElementRenderingSystem());
            //     scene.AddSystem(new PostProcessingSystem() { ExecutionOrder = -2 });

            Background.CreateBackground(scene, gaming);

            scene.AttachComponent(scene.CreateEntity(), new PostProcessingComponent
            {
                Effects = new List<IPostProcessingEffect>()
                {
                    new ShaderPostProcessor(Resources.Load<string>("shaders\\noise.frag")),
                },
                //End = new RenderOrder(100, 0)
            });

            scene.AttachComponent(scene.CreateEntity(), new PostProcessingComponent
            {
                Effects = new List<IPostProcessingEffect>()
                {
                    new ShaderPostProcessor(Resources.Load<string>("shaders\\chromatic_aberration.frag")),
                },
                //Begin = new RenderOrder(100, 1),
            });

            var orgin = scene.CreateEntity();
            scene.AttachComponent(orgin, new TransformComponent { Position = new Vector2(0, 0) });
            scene.AttachComponent(orgin, new RectangleShapeComponent { Color = Colors.Red });
            scene.AttachComponent(orgin, new WaveMovementComponent());

            var x100 = scene.CreateEntity();
            scene.AttachComponent(x100, new TransformComponent { Position = new Vector2(100, 0) });
            scene.AttachComponent(x100, new RectangleShapeComponent { Color = Colors.GreenYellow });
            scene.AttachComponent(x100, new WaveMovementComponent());

            var x50y50 = scene.CreateEntity();
            scene.AttachComponent(x50y50, new TransformComponent { Position = new Vector2(50, 50) });
            scene.AttachComponent(x50y50, new RectangleShapeComponent { Color = Colors.Blue });
            scene.AttachComponent(x50y50, new WaveMovementComponent());

            var agag = scene.CreateEntity();
            scene.AttachComponent(agag, new TransformComponent());
            var ccc = scene.AttachComponent(agag, new ElementComponent
            {
                Anchors = new IAnchor[]
                 {
                    new HorizontalCenterAnchor(),
                    new TopAnchor{Offset = 15},
                 },
                Size = new Vector2(128),
                StyleOverride = new Style
                {
                    BackgroundColour = new Property<Color>(Colors.Gray, Colors.Magenta, Colors.Magenta * 0.5f)
                },
                Overflow = OverflowBehaviour.Hide
            });

            ccc.SetTextString("hoe <color=#ff0000>is het </color><i>vandaag\nhawiudhauwdhawduawhdiauwhdaiuwhd");
            ccc.Text.Generator.ParseRichText = true;
            ccc.Text.Rebuild();

            var particles = scene.CreateEntity();
            scene.AttachComponent(particles, new TransformComponent());
            scene.AttachComponent(particles, new WaveMovementComponent());

            var opening = new Sound(Resources.Load<AudioData>("opening.wav"));
            game.AudioRenderer.PlayOnce(opening);

            var particleComponent = new ParticlesComponent(10000)
            {
                // Dampening = new FloatRange(0.9f),
                RotationalDampening = new FloatRange(0.95f),
                // Gravity = new Vec2Range(Vector2.Zero),
                StartVelocity = new Vec2Range(Vector2.One * -15, Vector2.One * 15),
                EmissionRate = 200,
                StartColor = new ColorRange(Colors.White),
                ColorOverLife = new ColorCurve(new Curve<Color>.Key(Colors.Green, 0), new Curve<Color>.Key(Color.Red, 1f)),
                SizeOverLife = new FloatCurve(new Curve<float>.Key(0, 0), new Curve<float>.Key(1, 0.1f), new Curve<float>.Key(0, 1)),
                LifeRange = new FloatRange(0.3f, .5f),
                WorldSpace = false,
                SimulationSpeed = 1
            };

            scene.AttachComponent(particles, particleComponent);

            //particleComponent.Material.SetUniform(ShaderDefaults.MainTextureUniform, gaming);

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

                // transform.Rotation += Time.UpdateDeltaTime * 64;
                DebugDraw.Circle(transform.Position, 3.4f, Colors.Green);
                //transform.Position = new Vector2(MathF.Sin(Time.SecondsSinceLoad * wave.Frequency + wave.Phase) * wave.Amplitude, 0);
                DebugDraw.Text(transform.Position, transform.Position.ToString(), 0.1f);
            }
            if (Input.IsKeyPressed(Key.E))
            {
                var opening = new Sound(Resources.Load<AudioData>("walgelijk.wav"));
                Audio.PlayOnce(opening);
            }

        }

        public override void Render()
        {
            //DebugDraw.Circle(Vector2.One, .4f, Colors.Green, renderOrder: new RenderOrder(101));

            //if (Utilities.RandomByte() != 200)
            //    return;

            //float l = 10;
            //var end = new Vector2(MathF.Cos(Time.SecondsSinceLoad), MathF.Sin(Time.SecondsSinceLoad)) * l;
            //DebugDraw.Line(default, end, duration: 1, renderOrder: new RenderOrder(101));
        }
    }
}
