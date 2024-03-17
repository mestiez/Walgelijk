using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK;

public readonly struct AudioStreamerHandle : IEquatable<AudioStreamerHandle>
{
    public readonly SourceHandle SourceHandle;
    public readonly Sound Sound;

    public AudioStreamerHandle(SourceHandle sourceHandle, Sound sound)
    {
        SourceHandle = sourceHandle;
        Sound = sound;
    }

    public override bool Equals(object obj)
    {
        return obj is AudioStreamerHandle handle && Equals(handle);
    }

    public bool Equals(AudioStreamerHandle other)
    {
        return SourceHandle.Equals(other.SourceHandle) &&
               EqualityComparer<Sound>.Default.Equals(Sound, other.Sound);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SourceHandle, Sound);
    }

    public static bool operator ==(AudioStreamerHandle left, AudioStreamerHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AudioStreamerHandle left, AudioStreamerHandle right)
    {
        return !(left == right);
    }
}
