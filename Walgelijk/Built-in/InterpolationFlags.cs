using System;

namespace Walgelijk;

/// <summary>
/// Interpolation bit mask for <see cref="TransformComponent"/>
/// </summary>
[Flags]
public enum InterpolationFlags : byte
{
    /// <summary>
    /// Interpolate nothing
    /// </summary>
    None = 0,

    /// <summary>
    /// Interpolate position
    /// </summary>
    Position = 2,
    /// <summary>
    /// Interpolate rotation
    /// </summary>
    Rotation = 4,
    /// <summary>
    /// Interpolate scale
    /// </summary>
    Scale = 8,
    /// <summary>
    /// Interpolate pivot
    /// </summary>
    LocalPivot = 16,
    /// <summary>
    /// Interpolate rotation pivot
    /// </summary>
    LocalRotationPivot = 32,

    /// <summary>
    /// Interpolate everything
    /// </summary>
    All = byte.MaxValue
}
