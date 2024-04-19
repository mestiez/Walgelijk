using NVorbis;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;

namespace Walgelijk.CommonAssetDeserialisers;

public class OggFixedAudioDeserialiser : IAssetDeserialiser
{
    public Type ReturningType => typeof(FixedAudioData);

    public bool IsCandidate(in AssetMetadata assetMetadata)
    {
        return
            assetMetadata.MimeType.Equals("audio/vorbis", StringComparison.InvariantCultureIgnoreCase) ||
            assetMetadata.MimeType.Equals("audio/ogg", StringComparison.InvariantCultureIgnoreCase);
    }

    public object Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var temp = stream();
        using var reader = new VorbisReader(temp);

        var sampleCount = reader.TotalSamples * reader.Channels;

        var buffer = new byte[sampleCount * 2]; // 16 bits per sample, in pairs of bytes
        var floatData = new float[sampleCount];

        int cPos = 0;
        while (true)
        {
            int count = reader.ReadSamples(floatData, cPos, floatData.Length);
            cPos += count;
            if (count <= 0 || count >= floatData.Length)
                break;
        }

        for (long i = 0; i < sampleCount; i++)
        {
            var sample = (short)(floatData[i] * short.MaxValue);
            buffer[i * 2] = (byte)sample;
            buffer[i * 2 + 1] = (byte)(sample >> 8);
        }

        return new FixedAudioData(buffer, reader.SampleRate, reader.Channels, reader.TotalSamples);
    }
}
