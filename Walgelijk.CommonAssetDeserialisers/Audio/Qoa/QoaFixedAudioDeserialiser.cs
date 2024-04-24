using Walgelijk.AssetManager.Deserialisers;
using Walgelijk.AssetManager;

// from https://github.com/pfusik/qoa-fu

namespace Walgelijk.CommonAssetDeserialisers.Audio.Qoa;

public class QoaFixedAudioDeserialiser : IAssetDeserialiser<FixedAudioData>
{
    public FixedAudioData Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var s = stream();
        var dec = new QOADecoder(s);

        if (!dec.ReadHeader())
            throw new Exception("Invalid header");

        int channelCount = dec.GetChannels();
        int totalSamples = dec.GetTotalSamples() * channelCount;

        var buffer = new short[totalSamples];
        int cursor = 0;

        while (true)
        {
            if (dec.IsEnd())
                break;

            int c = dec.ReadFrame(buffer.AsSpan(cursor..));

            cursor += c * channelCount;
        }

        Array.Resize(ref buffer, cursor);

        var data = new byte[buffer.Length * 2]; // 16 bits per sample, in pairs of bytes

        for (long i = 0; i < buffer.Length; i++)
        {
            var sample = buffer[i];
            data[i * 2] = (byte)sample;
            data[i * 2 + 1] = (byte)(sample >> 8);
        }

        var f = new FixedAudioData(data, dec.GetSampleRate(), totalSamples, totalSamples);
        return f;
    }

    public bool IsCandidate(in AssetMetadata assetMetadata)
    {
        return assetMetadata.Path.EndsWith("qoa", StringComparison.InvariantCultureIgnoreCase) ||
            assetMetadata.MimeType.Equals("audio/qoa", StringComparison.InvariantCultureIgnoreCase);
    }
}