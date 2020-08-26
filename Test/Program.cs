using System;
using System.Numerics;
using Walgelijk;
using Walgelijk.NAudio;
using Walgelijk.OpenTK;
using Walgelijk.Text;

namespace Test
{
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
            coolclip = Audio.LoadSound("audio\\cannot-build.wav");
            music = Audio.LoadSound("audio\\kampvuurliedlied.mp3");
            music.Looping = true;
        }

        public override void Render()
        {
            Program.coolSprite.SetUniform("time", Time.SecondsSinceStart * 12);
        }

        public override void Update()
        {
            //if (Input.TextEntered.Length != 0)
            //    Program.coolText.String += Input.TextEntered;

            if (Input.IsKeyPressed(Key.Down))
                Audio.Stop(ref music);

            if (Input.IsKeyPressed(Key.Up))
                Audio.Play(ref music);

            if (Input.IsKeyPressed(Key.Right))
                Audio.Pause(ref music);

            if (Input.IsKeyPressed(Key.M))
                Audio.Muted = !Audio.Muted;

            var zoomIn = Input.IsKeyHeld(Key.Plus);
            var zoomOut = Input.IsKeyHeld(Key.Minus);
            var cameraSystem = Scene.GetSystem<CameraSystem>();
            var cam = cameraSystem.MainCameraComponent;
            float factor = MathF.Pow(2f, Time.DeltaTime);
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

                cameraSystem.MainCameraTransform.Position = Vector2.Lerp(cameraSystem.MainCameraTransform.Position, transform.Position, Time.DeltaTime * 5);
                transform.Position += delta * player.MovementSpeed * Time.DeltaTime;
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

        static void Main(string[] args)
        {
            Random rand = new Random();

            Game game = new Game(
                new OpenTKWindow("hallo daar", new Vector2(128, 128), new Vector2(512, 512)),
                new NAudioRenderer()
                );

            game.Window.TargetFrameRate = 60;
            game.Window.TargetUpdateRate = 0;
            game.Window.VSync = false;
            game.Window.RenderTarget.ClearColour = new Color("#d42c5e");

            var scene = new Scene();
            game.Scene = scene;
            coolText = new TextComponent("hallo wereld!\nnieuwe regel...\nwat gebeurt er als ik \t doe", Font.Load("fonts\\broadway.fnt"));
            coolText.Tracking = .9f;

            {
                var ent = scene.CreateEntity();
                scene.AttachComponent(ent, new TransformComponent
                {
                    Scale = new Vector2(0.01f, -0.01f)
                });
                scene.AttachComponent(ent, coolText);
            }

            {
                var ent = scene.CreateEntity();
                scene.AttachComponent(ent, new TransformComponent
                {
                    Scale = new Vector2(0.015f, -0.015f),
                    Position = new Vector2(-10, 0)
                });
                scene.AttachComponent(ent, new TextComponent("andere font tijden\nrich text zou cool zijn... zo van\n<b>super</b> cool", Font.Load("fonts\\cambria.fnt")));
            }

            coolSprite = new Material(Shader.Load("shaders\\shader.vert", "shaders\\shader.frag"));
            coolSprite.SetUniform("texture1", Texture.Load("textures\\sadness.png"));
            coolSprite.SetUniform("texture2", Texture.Load("textures\\pride.png"));

            //create rectangles
            for (int i = 0; i < 2; i++)
            {
                var entity = scene.CreateEntity();

                scene.AttachComponent(entity, new TransformComponent
                {
                    Position = new Vector2(
                        Utilities.RandomFloat(-12f, 12f),
                        Utilities.RandomFloat(-7f, 7f)
                        ),
                    Rotation = i == 0 ? 0 : Utilities.RandomFloat(0, 360)
                });

                scene.AttachComponent(entity, new RectangleShapeComponent
                {
                    Size = Vector2.One * .5f,
                    Material = coolSprite
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

            game.Start();
        }
    }
}
