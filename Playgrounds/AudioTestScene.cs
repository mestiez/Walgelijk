using System.Numerics;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct AudioTestScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new AudioTestSystem());

        game.UpdateRate = 120;
        game.FixedUpdateRate = 50;

        return scene;
    }

    public class AudioTestSystem : Walgelijk.System
    {
        private Sound[] OneShot;

       // private Sound Streaming = new Sound(Resources.Load<StreamAudioData>("perfect-loop.ogg"), true, new SpatialParams(1, float.PositiveInfinity, 1));

        public AudioTestSystem()
        {
            OneShot = Assets.EnumerateFolder("one-shot").Select(asset =>
            {
                var sound = new Sound(Assets.Load<FixedAudioData>(asset), false, new SpatialParams(1, float.PositiveInfinity, 1));
                return sound;
            }).ToArray();
        }

        public override void FixedUpdate()
        {
            Audio.DistanceModel = AudioDistanceModel.InverseSquare;
            Audio.SpatialMultiplier = 0.1f;
            if (Input.IsKeyHeld(Key.Space))
            {
                Audio.PlayOnce(Utilities.PickRandom(OneShot), new Vector3(Utilities.RandomFloat(-100, 100), 0, 0), 0.1f);
                Audio.PlayOnce(Utilities.PickRandom(OneShot), new Vector3(Utilities.RandomFloat(-100, 100), 0, 0), 0.1f);
                Audio.PlayOnce(Utilities.PickRandom(OneShot), new Vector3(Utilities.RandomFloat(-100, 100), 0, 0), 0.1f);
                Audio.PlayOnce(Utilities.PickRandom(OneShot), new Vector3(Utilities.RandomFloat(-100, 100), 0, 0), 0.1f);
                Audio.PlayOnce(Utilities.PickRandom(OneShot), new Vector3(Utilities.RandomFloat(-100, 100), 0, 0), 0.1f);
                Audio.PlayOnce(Utilities.PickRandom(OneShot), new Vector3(Utilities.RandomFloat(-100, 100), 0, 0), 0.1f);
                Audio.PlayOnce(Utilities.PickRandom(OneShot), new Vector3(Utilities.RandomFloat(-100, 100), 0, 0), 0.1f);
            }
        }

        public override void Update()
        {
            var th = Time.SecondsSinceLoad;
            Audio.ListenerOrientation = (new Vector3(1 * MathF.Cos(th), 0, 1 * MathF.Sin(th)), Vector3.UnitY);
            //Audio.SetPosition(Streaming, new Vector3(0, 0, 100));

            //if (Input.IsKeyPressed(Key.Q))
            //    if (Audio.IsPlaying(Streaming))
            //        Audio.Pause(Streaming);
            //    else
            //        Audio.Play(Streaming);

            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Colour = Colors.Black;
            Draw.Quad(new Rect(0, 0, Window.Width, Window.Height));

            Draw.Colour = Colors.White;
            if (Audio is Walgelijk.OpenTK.OpenALAudioRenderer audio)
            {
                Draw.Text("Sources in use: " + audio.TemporarySourceBuffer.Count(), new Vector2(30, 30), Vector2.One,
                    HorizontalTextAlign.Left, VerticalTextAlign.Top);
                Draw.Text("Sources created: " + audio.CreatedTemporarySourceCount, new Vector2(30, 40), Vector2.One,
                    HorizontalTextAlign.Left, VerticalTextAlign.Top);
            }

            //var p = Audio.GetTime(Streaming) / (float)Streaming.Data.Duration.TotalSeconds;

            //Draw.Colour = Colors.Gray.Brightness(0.5f);
            //Draw.Quad(new Rect(30, 100, 30 + 500, 120));
            //Draw.Colour = Colors.Cyan;
            //Draw.Quad(new Rect(30, 100, 30 + 500 * p, 120).Expand(-2));
        }
    }
}