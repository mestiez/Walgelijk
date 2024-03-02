using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK;

public class OggStreamerCache : Cache<(SourceHandle, Sound), OggStreamer>
{
    protected override OggStreamer CreateNew((SourceHandle, Sound) raw)
    {
        return new OggStreamer(
            raw.Item1, 
            raw.Item2, 
            raw.Item2.Data as StreamAudioData ?? throw new global::System.Exception("OggStreamer created with non streaming audio data source"));
    }

    protected override void DisposeOf(OggStreamer loaded)
    {
        loaded.Dispose();
    }
}

public readonly struct OggStreamerHandle : IEquatable<OggStreamerHandle>
{
    public readonly SourceHandle SourceHandle;
    public readonly Sound Sound;

    public OggStreamerHandle(SourceHandle sourceHandle, Sound sound)
    {
        SourceHandle = sourceHandle;
        Sound = sound;
    }

    public override bool Equals(object obj)
    {
        return obj is OggStreamerHandle handle && Equals(handle);
    }

    public bool Equals(OggStreamerHandle other)
    {
        return SourceHandle.Equals(other.SourceHandle) &&
               EqualityComparer<Sound>.Default.Equals(Sound, other.Sound);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SourceHandle, Sound);
    }

    public static bool operator ==(OggStreamerHandle left, OggStreamerHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OggStreamerHandle left, OggStreamerHandle right)
    {
        return !(left == right);
    }
}
