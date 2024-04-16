﻿using System.Numerics;
using Walgelijk;
using Walgelijk.Onion;
using Walgelijk.SimpleDrawing;

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
            public int SampleRate = 44100;
            public long Position { get; set; }

            public int ReadSamples(Span<float> buffer)
            {
                // Position = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    double time = Position / (double)SampleRate;
                    var v = double.Sin(time * 250.0 * double.Tau);
                  
                    buffer[i] = (float)v;
                    
                    Position++;
                    TimePosition += TimeSpan.FromSeconds(1.0 / SampleRate);
                }

                return buffer.Length;
            }

            public TimeSpan TimePosition { get; set; }

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
        private Sound OneShot = new Sound(Resources.Load<FixedAudioData>("mono_hit.wav"), false,
            new SpatialParams(1, float.PositiveInfinity, 1));

        private Sound Streaming = new Sound(Resources.Load<StreamAudioData>("perfect-loop.ogg"), true,
            new SpatialParams(1, float.PositiveInfinity, 1));

        public override void FixedUpdate()
        {
            Audio.DistanceModel = AudioDistanceModel.InverseSquare;
            Audio.SpatialMultiplier = 0.1f;
            if (Input.IsKeyHeld(Key.Space))
            {
                Audio.PlayOnce(OneShot, new Vector3(Utilities.RandomFloat(-100, 100), 0, 0), 0.1f);
            }
        }

        public override void Update()
        {
            var th = Time.SecondsSinceLoad;
            Audio.ListenerOrientation = (new Vector3(1 * MathF.Cos(th), 0, 1 * MathF.Sin(th)), Vector3.UnitY);
            Audio.SetPosition(Streaming, new Vector3(0, 0, 100));

            if (Input.IsKeyPressed(Key.Q))
                if (Audio.IsPlaying(Streaming))
                    Audio.Pause(Streaming);
                else
                    Audio.Play(Streaming);

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

            var p = Audio.GetTime(Streaming) / (float)Streaming.Data.Duration.TotalSeconds;

            Draw.Colour = Colors.Gray.Brightness(0.5f);
            Draw.Quad(new Rect(30, 100, 30 + 500, 120));
            Draw.Colour = Colors.Cyan;
            Draw.Quad(new Rect(30, 100, 30 + 500 * p, 120).Expand(-2));
        }
    }
}