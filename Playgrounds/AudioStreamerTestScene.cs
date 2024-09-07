using Walgelijk;
using Walgelijk.Onion;

namespace Playgrounds;

public struct AudioStreamerTestScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TestSystem());
        scene.AddSystem(new OnionSystem());

        game.UpdateRate = 120;
        game.FixedUpdateRate = 50;

        return scene;
    }

    public class TestSystem : Walgelijk.System
    {
        public Sound[] Streams =
        [
            new Sound(Resources.Load<StreamAudioData>("perfect-loop.ogg"), loops: true),
            new Sound(Resources.Load<StreamAudioData>("mc4.ogg"), loops: false),
            new Sound(new StreamAudioData(() => new SineWaveStream(), 44100, 1, 441000), loops: false),
        ];

        private class SineWaveStream : IAudioStream
        {
            public long Position { get; set; }

            private double acc = 0;

            public int ReadSamples(Span<float> buffer)
            {

                // Position = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    var f = Utilities.MapRange(0, Walgelijk.Game.Main.Window.Height, 50.0, 450.0, Walgelijk.Game.Main.State.Input.WindowMousePosition.Y);
                    var v = double.Sin(acc * double.Tau);

                    buffer[i] = (float)v;

                    Position++;
                    TimePosition += TimeSpan.FromSeconds(1.0 / SampleRate);

                    acc += 1 / (double)SampleRate * f;
                }

                return buffer.Length;
            }

            public TimeSpan TimePosition { get; set; }

            public bool HasEnded => false;

            public int SampleRate => 44100;

            public int ChannelCount => 1;

            public void Dispose()
            {
            }
        }

        public override void Update()
        {
            // Stream.ForceUpdate();

            int i = 0;
            foreach (var s in Streams)
            {
                Ui.Layout.Size(512, 112);
                Ui.StartDragWindow("Controller", i++);
                {
                    Ui.Layout.Size(100, 32).Move(0, 0);
                    if (Ui.Button("Play"))
                        Audio.Play(s);

                    Ui.Layout.Size(100, 32).Move(100, 0);
                    if (Ui.Button("Pause"))
                        Audio.Pause(s);

                    Ui.Layout.Size(100, 32).Move(200, 0);
                    if (Ui.Button("Stop"))
                        Audio.Stop(s);

                    Ui.Layout.Size(200, 200).StickRight();
                    Ui.Label($"{(Audio.IsPlaying(s) ? "playing" : "stopped")}\n{Audio.GetTime(s)}");
                }
                Ui.End();
            }

            RenderQueue.Add(new ClearRenderTask());
        }
    }
}
