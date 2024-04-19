using NVorbis;
using System.Numerics;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.Onion;
using Walgelijk.OpenTK;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct AssetManagerTestScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        AssetDeserialisers.Register(new TestStreamAudioDeserialiser());
        Assets.RegisterPackage("assets.waa");

        var scene = new Scene(game);
        scene.AddSystem(new TestSystem());
        scene.AddSystem(new OnionSystem());

        game.UpdateRate = 120;
        game.FixedUpdateRate = 50;

        return scene;
    }


    public class TestStreamAudioDeserialiser : IAssetDeserialiser
    {
        public Type ReturningType => typeof(StreamAudioData);

        public bool IsCandidate(in AssetMetadata assetMetadata)
        {
            return
                assetMetadata.MimeType.Equals("audio/vorbis", StringComparison.InvariantCultureIgnoreCase) ||
                assetMetadata.MimeType.Equals("audio/ogg", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
        {
            var temp = Path.GetTempFileName();

            using var s = File.OpenWrite(temp);
            stream().CopyTo(s);
            s.Dispose();

            using var reader = new VorbisReader(temp);
            var data = VorbisFileReader.ReadMetadata(reader);
            reader.Dispose();
            return new StreamAudioData(() => new OggAudioStream(temp), data.SampleRate, data.NumChannels, data.SampleCount);
        }
    }

    public class TestSystem : Walgelijk.System
    {
        public Sound[] Streams =
        [
            new Sound(Assets.Load<StreamAudioData>("assets:danse_macabre.ogg").Value, loops: true),
            new Sound(Assets.Load<StreamAudioData>("assets:danse_macabre.ogg").Value, loops: true),
        ];

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
