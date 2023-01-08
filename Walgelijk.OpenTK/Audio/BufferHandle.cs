using System;

namespace Walgelijk.OpenTK;

public struct BufferHandle : IEquatable<BufferHandle>
{
    public int Handle;

    public bool Equals(BufferHandle other) => Handle == other.Handle;

    public override bool Equals(object obj) => obj is BufferHandle handle && Equals(handle);

    public static bool operator ==(BufferHandle left, BufferHandle right) => left.Equals(right);

    public static bool operator !=(BufferHandle left, BufferHandle right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(Handle);

    public static implicit operator int(BufferHandle entity) => entity.Handle;

    public static implicit operator BufferHandle(int handle) => new BufferHandle { Handle = handle };

    public override string ToString() => Handle.ToString();
}
