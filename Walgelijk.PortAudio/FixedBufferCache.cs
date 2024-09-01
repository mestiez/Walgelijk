using System.Buffers;
using System.Collections.Immutable;

namespace Walgelijk.PortAudio;

internal class FixedBufferCache : ConcurrentCache<FixedAudioData, ImmutableArray<float>>
{
    public static FixedBufferCache Shared { get; } = new();
 
    protected override ImmutableArray<float> CreateNew(FixedAudioData raw)
    {
        // TODO resampling

        ImmutableArray<float> d = [.. raw.Data];
        if (raw.DisposeLocalCopyAfterUpload)
            raw.Data = [];
        return d;
    }

    protected override void DisposeOf(ImmutableArray<float> loaded)
    {

    }
}
