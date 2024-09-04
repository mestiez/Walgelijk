using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.Onion;
using Walgelijk.Onion.Controls;

namespace Playgrounds;

public struct AssetManagerTestScene : ISceneCreator
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

    private static AudioTrack Music = new();
    private static AudioTrack SFX = new();

    public class TestSystem : Walgelijk.System
    {
        public (string Name, Sound Sound)[] Streams =
        [
            ("48000HzStream.ogg",new Sound(Assets.Load<StreamAudioData>("48000HzStream.ogg").Value) { Track = Music }),
            ("valve_machiavellian_bach.ogg", new Sound(Assets.Load<StreamAudioData>("valve_machiavellian_bach.ogg").Value, loops: true) { Track = Music }),
            ("sting_xp_level_up_orch_01.qoa", new Sound(Assets.Load<FixedAudioData>("sting_xp_level_up_orch_01.qoa").Value) { Track = SFX }),
            ("sample.wav", new Sound(Assets.Load<FixedAudioData>("sample.wav").Value) { Track = SFX }),
            ("perfect-loop.wav", new Sound(Assets.Load<FixedAudioData>("perfect-loop.wav").Value){ Looping = true, Track = SFX }),
            ("96000Hz.wav", new Sound(Assets.Load<FixedAudioData>("96000Hz.wav").Value) { Track = SFX }),
        ];

        public override void Update()
        {
            int i = 0;
            foreach (var device in Audio.EnumerateAvailableAudioDevices())
            {
                Ui.Layout.Size(200, 32).StickRight().StickTop().Move(0, i * 32);
                if (Audio.GetCurrentAudioDevice() == device)
                    Ui.Layout.Move(-10, 0);
                if (Ui.Button(device, i++))
                {
                    Audio.SetAudioDevice(device);
                }
            }

            Ui.Theme.Text(Colors.Black).Push();

            Ui.Layout.Size(50, 150).StickRight().StickBottom();
            float v = Music.Volume;
            if (Ui.FloatSlider(ref v, Direction.Vertical, (0,1), label: "{0:P0}\nMUSIC"))
                Music.Volume = v;

            Ui.Layout.Size(50, 150).StickRight().StickBottom().Move(-60, 0);
            v = SFX.Volume;
            if (Ui.FloatSlider(ref v, Direction.Vertical, (0,1), label: "{0:P0}\nSFX"))
                SFX.Volume = v;

            Ui.Theme.Pop();

            i = 0;
            foreach (var s in Streams)
            {
                Ui.Layout.Size(512, 112).Move(0, i * 112);
                Ui.StartDragWindow(s.Name, i++);
                {
                    Ui.Layout.Size(100, 32).Move(0, 0);
                    if (Ui.Button("Play"))
                        Audio.Play(s.Sound);

                    Ui.Layout.Size(100, 32).Move(100, 0);
                    if (Ui.Button("Pause"))
                        Audio.Pause(s.Sound);

                    Ui.Layout.Size(100, 32).Move(200, 0);
                    if (Ui.Button("Stop"))
                        Audio.Stop(s.Sound);

                    Ui.Layout.Size(100, 200).StickRight().StickTop();
                    Ui.Label($"{(Audio.IsPlaying(s.Sound) ? "playing" : "stopped")}\n{Audio.GetTime(s.Sound)}");

                    float time = Audio.GetTime(s.Sound);
                    Ui.Layout.FitWidth().Height(20).CenterHorizontal().StickBottom();
                    if (Ui.FloatSlider(ref time, Direction.Horizontal, (0, (float)s.Sound.Data.Duration.TotalSeconds), 0.01f))
                    {
                        Audio.SetTime(s.Sound, time);
                    }
                }
                Ui.End();
            }

            RenderQueue.Add(new ClearRenderTask());
        }
    }
}
