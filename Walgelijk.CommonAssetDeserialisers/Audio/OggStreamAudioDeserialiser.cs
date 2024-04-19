using NVorbis;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;

namespace Walgelijk.CommonAssetDeserialisers;

public class OggStreamAudioDeserialiser : IAssetDeserialiser
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
        return new StreamAudioData(() => new OggAudioStream(temp), reader.SampleRate, reader.Channels, reader.TotalSamples);
    }

    public class OggAudioStream : IAudioStream
    {
        private readonly VorbisReader reader;

        public OggAudioStream(string path)
        {
            reader = new VorbisReader(path);
        }

        public OggAudioStream(Stream source)
        {
            reader = new VorbisReader(source, false);
        }

        public long Position
        {
            get => reader.SamplePosition;
            set => reader.SamplePosition = value;
        }

        public TimeSpan TimePosition
        {
            get => reader.TimePosition;
            set => reader.TimePosition = value;
        }

        public int ReadSamples(Span<float> b) => reader.ReadSamples(b);

        public void Dispose()
        {
            reader?.Dispose();
        }
    }
}
