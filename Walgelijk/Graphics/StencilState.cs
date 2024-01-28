using System;

namespace Walgelijk;

public struct StencilState : IEquatable<StencilState>
{
    /// <summary>
    /// Is stencil writing/reading enabled at all? If false, the rest of the fields won't do anything.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// If true, the stencil buffer will be cleared to all zeroes
    /// </summary>
    public bool ShouldClear;

    /// <summary>
    /// The current stencil access mode, see <see cref="StencilAccessMode"/>
    /// </summary>
    public StencilAccessMode AccessMode;
    public StencilTestMode TestMode;

    /// <summary>
    /// Disable stencil testing and writing
    /// </summary>
    public static StencilState Disabled => new() { Enabled = false };

    /// <summary>
    /// Clear the stencil buffer to all zeroes
    /// </summary>
    public static StencilState Clear => new() { Enabled = true, ShouldClear = true };

    /// <summary>
    /// Any geometry drawn will result in 1s on the stencil buffer, i.e determines the mask
    /// </summary>
    public static StencilState WriteMask => new() { Enabled = true, AccessMode = StencilAccessMode.Write };

    /// <summary>
    /// Fragments will only be drawn if the stencil buffer at that point is 1, i.e inside the mask
    /// </summary>
    public static StencilState InsideMask => new() { Enabled = true, AccessMode = StencilAccessMode.NoWrite, TestMode = StencilTestMode.Inside };

    /// <summary>
    /// Fragments will only be drawn if the stencil buffer at that point is 0, i.e outside the mask
    /// </summary>
    public static StencilState OutsideMask => new() { Enabled = true, AccessMode = StencilAccessMode.NoWrite, TestMode = StencilTestMode.Outside };

    public override bool Equals(object? obj)
    {
        return obj is StencilState state && Equals(state);
    }

    public bool Equals(StencilState other)
    {
        return Enabled == other.Enabled &&
               ShouldClear == other.ShouldClear &&
               AccessMode == other.AccessMode &&
               TestMode == other.TestMode;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enabled, ShouldClear, AccessMode, TestMode);
    }

    public static bool operator ==(StencilState left, StencilState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StencilState left, StencilState right)
    {
        return !(left == right);
    }
}
