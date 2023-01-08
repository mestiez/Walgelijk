using System;

namespace Walgelijk.OpenTK;

public struct SourceHandle : IEquatable<SourceHandle>
{
    public int Handle;

    public bool Equals(SourceHandle other) => Handle == other.Handle;

    public override bool Equals(object obj) => obj is SourceHandle handle && Equals(handle);

    public static bool operator ==(SourceHandle left, SourceHandle right) => left.Equals(right);

    public static bool operator !=(SourceHandle left, SourceHandle right) => !(left == right);

    public static implicit operator int(SourceHandle entity) => entity.Handle;

    public static implicit operator SourceHandle(int handle) => new SourceHandle { Handle = handle };

    public override string ToString() => Handle.ToString();

    public override int GetHashCode() => HashCode.Combine(Handle);
}
