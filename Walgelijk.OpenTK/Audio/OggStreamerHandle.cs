using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK;

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
