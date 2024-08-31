using NVorbis;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;

namespace Walgelijk.CommonAssetDeserialisers.Audio;

public class OggFixedAudioDeserialiser : IAssetDeserialiser<FixedAudioData>
{
    public bool IsCandidate(in AssetMetadata assetMetadata)
    {
        return
            assetMetadata.MimeType.Equals("audio/vorbis", StringComparison.InvariantCultureIgnoreCase) ||
            assetMetadata.MimeType.Equals("audio/ogg", StringComparison.InvariantCultureIgnoreCase);
    }

    public FixedAudioData Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var temp = stream();
        using var reader = new VorbisReader(temp, false);

        var sampleCount = reader.TotalSamples * reader.Channels;

        var floatData = new float[sampleCount];

        int cPos = 0;
        while (true)
        {
            int count = reader.ReadSamples(floatData, cPos, floatData.Length);
            cPos += count;
            if (count <= 0 || count >= floatData.Length)
                break;
        }

        return new FixedAudioData(floatData, reader.SampleRate, reader.Channels, reader.TotalSamples);
    }
}
