using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Walgelijk;
using Walgelijk.NAudio;
using Walgelijk.OpenTK;

namespace Test
{
    //Deze hele file is om shit te testen, negeer het

    public static class OtherCoolThing
    {
        public static Scene CreateGameScene()
        {
            Scene scene = new Scene();

            var camera = scene.CreateEntity();
            scene.AttachComponent(camera, new TransformComponent());
            scene.AttachComponent(camera, new CameraComponent());

            CreateDebugGrid(scene, 4, 4, 2);

            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new DebugCameraSystem());
            scene.AddSystem(new ShapeRendererSystem());
            scene.AddSystem(new CameraSystem());

            return scene;
        }

        private static void CreateDebugGrid(Scene scene, int width = 8, int height = 8, int step = 10)
        {
            int xExtent = width / 2;
            int yExtent = height / 2;

            for (int y = -yExtent; y < yExtent + 1; y++)
            {
                for (int x = -xExtent; x < xExtent + 1; x++)
                {
                    var point = scene.CreateEntity();
                    var pos = new Vector2(x, y) * step;
                    scene.AttachComponent(point, new TransformComponent { Scale = new Vector2(0.04f, 0.04f), Position = pos });
                    scene.AttachComponent(point, new RectangleShapeComponent());
                    scene.AttachComponent(point, new TextComponent($"{pos.X}, {pos.Y}"));

                    //var dot = scene.CreateEntity();
                    //scene.AttachComponent(dot, new TransformComponent { Scale = new Vector2(0.1f, 0.1f), Position = pos });
                }
            }
        }
    }

    public class PlayerComponent
    {
        public float MovementSpeed = 4f;
    }

    public class PlayerSystem : Walgelijk.System
    {
        private Sound coolclip;
        private Sound music;

        public override void Initialise()
        {
            coolclip = Resources.Load<Sound>("cannot-build.wav");
            music = Resources.Load<Sound>("kampvuurliedlied.mp3");
            music.Looping = true;
        }

        public override void Render()
        {
            Program.coolSprite.SetUniform("time", Time.SecondsSinceLoad * 12);
        }

        public override void Update()
        {

            if (Input.IsKeyPressed(Key.Down))
                Audio.Stop(ref music);

            if (Input.IsKeyPressed(Key.Up))
                Audio.Play(ref music);

            if (Input.IsKeyPressed(Key.Right))
                Audio.Pause(ref music);

            if (Input.IsKeyPressed(Key.M))
                Audio.Muted = !Audio.Muted;

            if (Input.IsKeyPressed(Key.K))
                Game.Main.Scene = OtherCoolThing.CreateGameScene();

            if (Input.IsKeyHeld(Key.R))
                Logger.Warn("Warning message at " + Time.SecondsSinceLoad);

            var zoomIn = Input.IsKeyHeld(Key.Plus);
            var zoomOut = Input.IsKeyHeld(Key.Minus);
            var cameraSystem = Scene.GetSystem<CameraSystem>();
            var cam = cameraSystem.MainCameraComponent;
            float factor = MathF.Pow(2f, Time.UpdateDeltaTime);
            if (zoomIn)
                cam.OrthographicSize /= factor;
            else if (zoomOut)
                cam.OrthographicSize *= factor;

            var w = Input.IsKeyHeld(Key.W);
            var a = Input.IsKeyHeld(Key.A);
            var s = Input.IsKeyHeld(Key.S);
            var d = Input.IsKeyHeld(Key.D);

            var delta = Vector2.Zero;

            if (w) delta.Y = 1;
            if (a) delta.X = -1;
            if (s) delta.Y -= 1;
            if (d) delta.X += 1;

            if (delta.Length() >= 1)
                delta = Vector2.Normalize(delta);

            foreach (var pair in Scene.GetAllComponentsOfType<PlayerComponent>())
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(pair.Entity);
                PlayerComponent player = pair.Component;

                cameraSystem.MainCameraTransform.Position = Vector2.Lerp(cameraSystem.MainCameraTransform.Position, transform.Position, Time.UpdateDeltaTime * 5);
                transform.Position += delta * player.MovementSpeed * Time.UpdateDeltaTime;
                transform.Rotation += Utilities.DeltaAngle(transform.Rotation, MathF.Atan2(delta.Y, delta.X) * Utilities.RadToDeg) * Time.UpdateDeltaTime * 10;
                if (Input.IsKeyReleased(Key.O))
                    Scene.RemoveEntity(pair.Entity);

                if (Input.IsKeyPressed(Key.Space))
                {
                    Audio.PlayOnce(coolclip);
                    var entity = Scene.CreateEntity();

                    Scene.AttachComponent(entity, new TransformComponent
                    {
                        Position = transform.Position
                    });

                    Scene.AttachComponent(entity, new RectangleShapeComponent
                    {
                        Size = Vector2.One * .5f,
                        Material = Program.coolSprite
                    });
                }
            }
        }
    }

    class Program
    {
        public static Material coolSprite;
        public static TextComponent coolText;
        private static Game game;

        static void Main(string[] args)
        {
            //float odds = 0;
            //const int iterations = 1000;
            //for (int i = 0; i < iterations; i++)
            //{
            //    HashSet<int> existing = new HashSet<int>();
            //    while (true)
            //    {
            //        int id = IdentityGenerator.Generate();
            //        if (existing.Contains(id))
            //        {
            //            odds += (1f / existing.Count) / iterations;
            //            break;
            //        }
            //        else
            //            existing.Add(id);
            //    }
            //}

            //Logger.Log(odds * 100 + "% probability of ID collision");



            game = new Game(
                new OpenTKWindow("hallo daar", new Vector2(-1, -1), new Vector2(800, 600)),
                new NAudioRenderer()
                );

            game.Window.TargetFrameRate = 0;
            game.Window.TargetUpdateRate = 0;
            game.Window.VSync = false;
            game.Window.RenderTarget.ClearColour = new Color("#d42c5e");

            Resources.SetBasePathForType<Sound>("audio");
            Resources.SetBasePathForType<Prefab>("prefabs");
            Resources.SetBasePathForType<Texture>("textures");
            Resources.SetBasePathForType<IReadableTexture>("textures");
            Resources.SetBasePathForType<IWritableTexture>("textures");
            Resources.SetBasePathForType<Font>("fonts");

            game.Scene = SplashScreen.CreateScene(new[] {
                new SplashScreen.Logo(Resources.Load<IReadableTexture>("walgelijk.png"), 0.5f, Resources.Load<Sound>("walgelijk.wav")),
               // new SplashScreen.Logo(Resources.Load<Texture>("studio minus.png"), 2.1f, Resources.Load<Sound>("opening.wav")),
               // new SplashScreen.Logo(Resources.Load<IReadableTexture>("spellejte.png")),
            }, LoadScene);

            //  LoadScene();

            game.Start();
        }

        private static void LoadScene()
        {
            game.Window.RenderTarget.ClearColour = Color.Blue;
            var scene = new Scene();
            game.Scene = scene;
            coolText = new TextComponent("hallo wereld!\nnieuwe regel...\nwat gebeurt er als ik \t doe", Resources.Load<Font>("inter.fnt"));
            coolText.TrackingMultiplier = .9f;
            coolText.RenderOrder = -1000;

            //{
            //    var ent = scene.CreateEntity();
            //    scene.AttachComponent(ent, new TransformComponent
            //    {
            //        Scale = new Vector2(0.01f, 0.01f)
            //    });
            //    scene.AttachComponent(ent, coolText);
            //}

            //{
            //    var ent = scene.CreateEntity();
            //    scene.AttachComponent(ent, new TransformComponent
            //    {
            //        Scale = new Vector2(0.015f, 0.015f),
            //        Position = new Vector2(-10, 0)
            //    });
            //    scene.AttachComponent(ent, new TextComponent("andere font tijden\nrich text zou cool zijn... zo van\n<b>super</b> cool", Resources.Load<Font>("broadway.fnt")/*, Font.Load("fonts\\kosugi maru.fnt")*/));
            //}

            coolSprite = new Material(new Shader(Resources.Load<string>("shaders/shader.vert"), Resources.Load<string>("shaders/shader.frag")));
            coolSprite.SetUniform("texture1", Resources.Load<Texture>("sadness.png"));
            coolSprite.SetUniform("texture2", Resources.Load<Texture>("pride.png"));
            Entity player = default;
            //create rectangles
            for (int i = 0; i < 1; i++)
            {
                var entity = scene.CreateEntity();

                scene.AttachComponent(entity, new TransformComponent
                {
                    Position = i == 0 ? Vector2.Zero : new Vector2(
                        Utilities.RandomFloat(-25f, 25f),
                        Utilities.RandomFloat(-25f, 25f)
                        ),
                    Rotation = i == 0 ? 0 : Utilities.RandomFloat(0, 360)
                });

                scene.AttachComponent(entity, new QuadShapeComponent(true)
                {
                    Material = coolSprite,
                    RenderOrder = (i == 0) ? 1 : 0,
                });

                if (i == 0)
                {
                    player = entity;
                    scene.AttachComponent(entity, new PlayerComponent());
                }
            }

            for (int i = 0; i < 5; i++)
            {
                var entity = scene.CreateEntity();

                scene.AttachComponent(entity, new TransformComponent
                {
                    Position = new Vector2(
                        Utilities.RandomFloat(-2f, 2f),
                        Utilities.RandomFloat(-2f, 2f)
                        ),
                    Scale = new Vector2(0.5f),
                    Rotation = Utilities.RandomFloat(0, 360),
                    Parent = player
                });

                scene.AttachComponent(entity, new QuadShapeComponent(true)
                {
                    Material = coolSprite,
                    RenderOrder = 0,
                });

                scene.AttachComponent(entity, new SpinnyComponent
                {
                    Speed = Utilities.RandomFloat(-64, 64)
                });

                for (int ii = 0; ii < 5; ii++)
                {
                    var ee = scene.CreateEntity();

                    scene.AttachComponent(ee, new TransformComponent
                    {
                        Position = new Vector2(
                            Utilities.RandomFloat(0, 1f),
                            0
                            ),
                        Rotation = Utilities.RandomFloat(0, 360),
                        Scale = new Vector2(0.25f),
                        Parent = entity
                    });

                    scene.AttachComponent(ee, new QuadShapeComponent(true)
                    {
                        Material = coolSprite,
                        RenderOrder = 0,
                    });
                }
            }

            //create camera
            {
                //var entity = scene.CreateEntity();
                //scene.AttachComponent(entity, new CameraComponent());
                //scene.AttachComponent(entity, new TransformComponent());

                Resources.Load<Prefab>("camera.json").UnpackTo(scene);

                var cameraSystem = new CameraSystem();
                scene.AddSystem(cameraSystem);
                //cameraSystem.SetMainCamera(entity);

            }

            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new ShapeRendererSystem());
            scene.AddSystem(new PlayerSystem());
            scene.AddSystem(new SpinnySystem());
        }
    }

    [RequiresComponents(typeof(TransformComponent))]
    public class SpinnyComponent
    {
        public float Speed = 64;
    }

    public class SpinnySystem : Walgelijk.System
    {
        public override void Update()
        {
            var pairs = Scene.GetAllComponentsOfType<SpinnyComponent>();
            foreach (var item in pairs)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(item.Entity);

                transform.Rotation += item.Component.Speed * Time.UpdateDeltaTime;
            }
        }

        [Command]
        public static void gaming()
        {
            Logger.Log("Logica werkt");
        }
    }
}
