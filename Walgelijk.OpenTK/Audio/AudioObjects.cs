using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Walgelijk.OpenTK;

internal static class AudioObjects
{
    public static readonly FixedAudioCache FixedBuffers = new();
    public static readonly SourceCache Sources = new();
    public static readonly OggStreamerCache OggStreamers = new();

    public static readonly ConcurrentDictionary<BufferHandle, List<SourceHandle>> SourceByBuffer = new();
}
