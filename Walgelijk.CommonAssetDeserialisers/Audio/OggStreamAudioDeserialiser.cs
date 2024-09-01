using NVorbis;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;

namespace Walgelijk.CommonAssetDeserialisers.Audio;

public class OggStreamAudioDeserialiser : IAssetDeserialiser<StreamAudioData>
{
    public bool IsCandidate(in AssetMetadata assetMetadata)
    {
        return
            assetMetadata.MimeType.Equals("audio/vorbis", StringComparison.InvariantCultureIgnoreCase) ||
            assetMetadata.MimeType.Equals("audio/ogg", StringComparison.InvariantCultureIgnoreCase);
    }

    public StreamAudioData Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        var reader = new VorbisReader(stream(), false);
        reader.Initialize();
        return new StreamAudioData(() => new OggAudioStream(reader), reader.SampleRate, reader.Channels, reader.TotalSamples);
    }

    public class OggAudioStream : IAudioStream
    {
        private readonly VorbisReader reader;
        private readonly double SecondsPerSample;

        public OggAudioStream(string path)
        {
            reader = new VorbisReader(path);
            reader.Initialize();

            SecondsPerSample = 1d/ reader.SampleRate;
        }

        public OggAudioStream(Stream source)
        {
            reader = new VorbisReader(source, false);
            reader.Initialize();

            SecondsPerSample = 1d/ reader.SampleRate;
        }

        public OggAudioStream(VorbisReader reader)
        {
            this.reader = reader;

            SecondsPerSample = 1d/ reader.SampleRate;
        }

        public long Position
        {
            get => reader.SamplePosition;
            set => reader.SamplePosition = long.Clamp(value, 0, reader.TotalSamples - 1);
        }

        public TimeSpan TimePosition
        {
            get => TimeSpan.FromSeconds(Position * SecondsPerSample); // dit is een decoded time position, niet sample time
            set => Position = (long)(value.TotalSeconds * reader.SampleRate);
        }

        public bool HasEnded => reader.IsEndOfStream;

        public int ReadSamples(Span<float> b) => reader.ReadSamples(b);

        public void Dispose()
        {
            reader?.Dispose();
        }
    }
}
