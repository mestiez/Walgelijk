using NVorbis;
using System.Numerics;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;
using Walgelijk.CommonAssetDeserialisers;
using Walgelijk.CommonAssetDeserialisers.Audio;
using Walgelijk.CommonAssetDeserialisers.Audio.Qoa;
using Walgelijk.Onion;
using Walgelijk.OpenTK;
using Walgelijk.SimpleDrawing;

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

    public class TestSystem : Walgelijk.System 
    {
        public (string Name, Sound Sound)[] Streams =
        [
            //new Sound(Assets.Load<StreamAudioData>("assets:danse_macabre.ogg").Value, loops: true),
            ("valve_machiavellian_bach.ogg", new Sound(Assets.Load<StreamAudioData>("valve_machiavellian_bach.ogg").Value, loops: true)),
            ("sting_xp_level_up_orch_01.qoa", new Sound(Assets.Load<FixedAudioData>("sting_xp_level_up_orch_01.qoa").Value)),
            ("sample.wav", new Sound(Assets.Load<FixedAudioData>("sample.wav").Value)),
            ("perfect-loop.wav", new Sound(Assets.Load<FixedAudioData>("perfect-loop.wav").Value){ Looping = true }),
            ("ppg_alarm_2.wav", new Sound(Assets.Load<FixedAudioData>("ppg_alarm_2.wav").Value)),
        ];

        public override void Update()
        {
            // Stream.ForceUpdate();

            int i = 0;
            foreach (var s in Streams)
            {
                Ui.Layout.Size(512, 112);
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

                    Ui.Layout.Size(200, 200).StickRight();
                    Ui.Label($"{(Audio.IsPlaying(s.Sound) ? "playing" : "stopped")}\n{Audio.GetTime(s.Sound)}");
                }
                Ui.End();
            }

            RenderQueue.Add(new ClearRenderTask());
        }
    }
}
