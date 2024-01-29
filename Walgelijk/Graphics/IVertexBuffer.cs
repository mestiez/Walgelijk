using System.Buffers;

namespace Walgelijk;

/// <summary>
/// Provides functions that are common to all vertex buffers and required for the graphics backend
/// </summary>
public interface IVertexBuffer
{
    /// <summary>
    /// Returns a <see cref="MemoryHandle"/> to the contiguous block of memory that stores all vertices, whatever type they may be
    /// </summary>
    /// <returns></returns>
    public MemoryHandle GetVerticesMemoryHandle();

    /// <summary>
    /// Amount of vertices
    /// </summary>
    public int VertexCount { get; }

    /// <summary>
    /// The descriptor that provides functions to describe the vertex object to the graphics backend
    /// </summary>
    public IVertexDescriptor Descriptor { get; }
}
