using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;

namespace Walgelijk.CommonAssetDeserialisers.Audio;

public class WaveFixedAudioDeserialiser : IAssetDeserialiser<FixedAudioData>
{
    public bool IsCandidate(in AssetMetadata assetMetadata)
    {
        return
            assetMetadata.MimeType.Equals("audio/wav", StringComparison.InvariantCultureIgnoreCase) ||
            assetMetadata.MimeType.Equals("audio/x-wav", StringComparison.InvariantCultureIgnoreCase);
    }

    public FixedAudioData Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var input = new BinaryReader(stream(), global::System.Text.Encoding.ASCII, leaveOpen: false);

        if (!input.SkipUntil("RIFF"))
            throw new Exception("Input file is not a WAVE file: no RIFF header");

        if (!input.SkipUntil("WAVE"))
            throw new Exception("Input file is not a WAVE file: format does not equal \"WAVE\"");

        if (!input.ReadChars(4).SequenceEqual("fmt "))
            throw new Exception("Input file is not a WAVE file: \"fmt \" chunk not found");

        if (input.ReadInt32() != 16)
            throw new Exception("Input file is not a valid WAVE file: fmt chunk does not equal 16");

        if (input.ReadInt16() != 1)
            throw new Exception("Input file is not a supported WAVE file: AudioFormat does not equal 1, data is compressed");

        // read channel count
        var numChannels = input.ReadInt16();
        if (numChannels is not (1 or 2))
            throw new Exception("Input WAVE file has an invalid channel count. The number of channels must be 1 or 2");

        // read sample rate
        var sampleRate = input.ReadInt32();
        if (sampleRate <= 0 || sampleRate > 96000)
            throw new Exception($"Input WAVE file has an invalid sample rate of ({sampleRate}). The sample rate must be greater than 0 and less than or equal to 96000");

        input.ReadInt32(); // ignore byte rate
        input.ReadInt16(); // ignore block align

        var bitsPerSample = input.ReadInt16();
        if (bitsPerSample != 16)
            throw new Exception($"Input file is not a valid WAVE file: the engine only supports 16 bit WAVE files, this file is {bitsPerSample} bit");

        var buffer = new byte[4];
        var dataChunk = global::System.Text.Encoding.ASCII.GetBytes("data");
        // find data chunk
        while (true)
        {
            // find data chunk
            var b = input.Read(buffer);
            if (b != 4)
                throw new Exception("Input file is not a WAVE file: Data chunk could not be found");

            if (buffer.SequenceEqual(dataChunk))
                break;
        }

        int dataChunkSize = input.ReadInt32();

        // read actual audio data
        var data = new byte[dataChunkSize];
        int actuallyRead = input.Read(data, 0, dataChunkSize);
        if (actuallyRead != dataChunkSize)
            throw new Exception("Input file is not a valid WAVE file: the reported data chunk size does not match the actually read data size");

        // 16 bit audio, so we have to process two bytes per sample
        int bi = 0;
        var floatData = new float[data.Length / 2];
        for (int i = 0; i < floatData.Length; i++)
        {
            var sample = BitConverter.ToInt16(data, bi);
            bi += 2;

            floatData[i] = sample / (float)short.MaxValue;
        }

        return new FixedAudioData(floatData, sampleRate, numChannels, floatData.Length / numChannels);
    }
}
