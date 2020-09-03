using System;
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
            coolclip = Resources.Load<Sound>("audio\\cannot-build.wav");
            music = Resources.Load<Sound>("audio\\kampvuurliedlied.mp3");
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
            game = new Game(
                new OpenTKWindow("hallo daar", new Vector2(-1, -1), new Vector2(800, 600)),
                new NAudioRenderer()
                );

            game.Window.TargetFrameRate = 0;
            game.Window.TargetUpdateRate = 0;
            game.Window.VSync = false;
            game.Window.RenderTarget.ClearColour = new Color("#d42c5e");

            game.Scene = SplashScreen.CreateScene(new[] {
                new SplashScreen.Logo(Resources.Load<Texture>("textures\\walgelijk.png"), sound: Resources.Load<Sound>("audio\\walgelijk.wav")),
                new SplashScreen.Logo(Resources.Load<Texture>("textures\\studio minus.png"), 2.1f, sound: Resources.Load<Sound>("audio\\opening.wav")),
                new SplashScreen.Logo(Resources.Load<Texture>("textures\\spellejte.png")),
            }, LoadScene);

          //  LoadScene();

            game.Start();
        }

        private static void LoadScene()
        {
            game.Window.RenderTarget.ClearColour = Color.Blue;
            var scene = new Scene();
            game.Scene = scene;
            coolText = new TextComponent("hallo wereld!\nnieuwe regel...\nwat gebeurt er als ik \t doe", Resources.Load<Font>("fonts\\inter.fnt"));
            coolText.TrackingMultiplier = .9f;

            {
                var ent = scene.CreateEntity();
                scene.AttachComponent(ent, new TransformComponent
                {
                    Scale = new Vector2(0.01f, 0.01f)
                });
                scene.AttachComponent(ent, coolText);
            }

            {
                var ent = scene.CreateEntity();
                scene.AttachComponent(ent, new TransformComponent
                {
                    Scale = new Vector2(0.015f, 0.015f),
                    Position = new Vector2(-10, 0)
                });
                scene.AttachComponent(ent, new TextComponent("andere font tijden\nrich text zou cool zijn... zo van\n<b>super</b> cool", Resources.Load<Font>("fonts\\broadway.fnt")/*, Font.Load("fonts\\kosugi maru.fnt")*/));
            }

            coolSprite = new Material(new Shader(Resources.Load<string>("shaders\\shader.vert"), Resources.Load<string>("shaders\\shader.frag")));
            coolSprite.SetUniform("texture1", Resources.Load<Texture>("textures\\sadness.png"));
            coolSprite.SetUniform("texture2", Resources.Load<Texture>("textures\\pride.png"));

            //create rectangles
            for (int i = 0; i < 200; i++)
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

                scene.AttachComponent(entity, new RectangleShapeComponent
                {
                    Size = Vector2.One * .5f,
                    Material = coolSprite,
                    RenderOrder = (i == 0) ? 1 : 0,
                });

                if (i == 0)
                    scene.AttachComponent(entity, new PlayerComponent());
            }

            //create camera
            {
                var entity = scene.CreateEntity();
                scene.AttachComponent(entity, new CameraComponent());
                scene.AttachComponent(entity, new TransformComponent());

                var cameraSystem = new CameraSystem();
                scene.AddSystem(cameraSystem);
                cameraSystem.SetMainCamera(entity);
            }

            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new ShapeRendererSystem());
            scene.AddSystem(new PlayerSystem());

        }
    }
}
