using System.Buffers;
using System.Collections.Immutable;

namespace Walgelijk.PortAudio;

internal class FixedBufferCache : ConcurrentCache<FixedAudioData, ImmutableArray<float>>
{
    public static FixedBufferCache Shared { get; } = new();

    protected override ImmutableArray<float> CreateNew(FixedAudioData raw)
    {
        float[] data = raw.Data;

        if (raw.SampleRate != PortAudioRenderer.SampleRate) // we gotta resample
        {
            float[] resampled = new float[Resampler.GetOutputLength(data.Length, raw.SampleRate, PortAudioRenderer.SampleRate, raw.ChannelCount)];
            if (raw.ChannelCount == 2)
                Resampler.ResampleInterleavedStereo(data, resampled, raw.SampleRate, PortAudioRenderer.SampleRate);
            else
                Resampler.ResampleMono(data, resampled, raw.SampleRate, PortAudioRenderer.SampleRate);
            data = resampled;
        }

        ImmutableArray<float> d = [.. data];
        if (raw.DisposeLocalCopyAfterUpload)
            raw.Data = [];
        return d;
    }

    protected override void DisposeOf(ImmutableArray<float> loaded)
    {

    }
}
