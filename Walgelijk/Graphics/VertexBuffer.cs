using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Walgelijk;

/// <summary>
/// Holds all the data needed to draw vertices to the screen
/// </summary>
public class VertexBuffer<TVertex> : IDisposable, IVertexBuffer where TVertex : struct
{
    private TVertex[] vertices = [];
    private uint[] indices = [];
    private bool disposed = false;

    private MemoryHandle verticesHandle;

    public bool Disposed => disposed;

    private readonly VertexAttributeArray[]? extraAttributes = null;

    /// <summary>
    /// The way vertices are drawn
    /// </summary>
    public Primitive PrimitiveType { get; set; } = Primitive.Triangles;

    /// <summary>
    /// Create a VertexBuffer with the specified vertices and indices
    /// </summary>
    public VertexBuffer(TVertex[] vertices, uint[] indices, IVertexDescriptor vertexDescriptor, VertexAttributeArray[]? extraAttributes = null)
    {
        this.vertices = vertices;
        this.indices = indices;
        this.extraAttributes = extraAttributes;
        Descriptor = vertexDescriptor;

        verticesHandle = vertices.AsMemory().Pin();
    }

    /// <summary>
    /// Create a VertexBuffer with the specified vertices. The indices will be set automatically
    /// </summary>
    public VertexBuffer(TVertex[] vertices, IVertexDescriptor vertexDescriptor)
    {
        Vertices = vertices;
        verticesHandle = vertices.AsMemory().Pin();
        Descriptor = vertexDescriptor;

        GenerateIndices();
    }

    /// <summary>
    /// Create an empty vertex buffer
    /// </summary>
    public VertexBuffer()
    {
    }

    /// <summary>
    /// If non-null, this determines the amount of indices to render. If null, the renderer will fall back to the amount of indices in the <see cref="Indices"/> array. <b>This should NOT exceed the amount of available indices</b>
    /// </summary>
    public int? AmountOfIndicesToRender = null;

    /// <summary>
    /// Whether the data needs to be uploaded to the GPU again
    /// </summary>
    public bool HasChanged { get; set; } = false;

    /// <summary>
    /// Whether the extra data needs to be uploaded to the GPU again
    /// </summary>
    public bool ExtraDataHasChanged { get; set; } = false;

    /// <summary>
    /// Does the vertex data change often?
    /// </summary>
    public bool Dynamic { get; set; }

    /// <summary>
    /// The local arrays will be cleared and made unusable after upload to the GPU
    /// </summary>
    public bool DisposeLocalCopy { get; set; } = false;

    /// <summary>
    /// Vertices to draw. <b>Do not forget to set the corresponding indices, or use <see cref="GenerateIndices"/></b>
    /// </summary>
    public TVertex[] Vertices
    {
        get => vertices;

        set
        {
            vertices = value;
            HasChanged = true;
            verticesHandle.Dispose();
            verticesHandle = vertices.AsMemory().Pin();
        }
    }

    /// <summary>
    /// Indices to draw vertices by
    /// </summary>
    public uint[] Indices
    {
        get => indices;

        set
        {
            indices = value;
            HasChanged = true;
        }
    }

    /// <summary>
    /// Amount of indices
    /// </summary>
    public int IndexCount => Indices?.Length ?? 0;

    /// <summary>
    /// Amount of vertices
    /// </summary>
    public int VertexCount => Vertices?.Length ?? 0;

    /// <summary>
    /// Force the data to be reuploaded to the GPU
    /// </summary>
    public void ForceUpdate()
    {
        HasChanged = true;
    }

    /// <summary>
    /// Generates indices that simply walk the vertex array from beginning to end
    /// </summary>
    public void GenerateIndices()
    {
        indices = new uint[vertices.Length];
        for (uint i = 0; i < vertices.Length; i++)
            indices[i] = i;
        HasChanged = true;
    }

    /// <summary>
    /// Find the location of a vertex attribute array with the given type. Returns -1 if nothing is found.
    /// </summary>
    public int FindAttribute(AttributeType t)
    {
        if (extraAttributes == null)
            return -1;

        for (int i = 0; i < extraAttributes.Length; i++)
        {
            var item = extraAttributes[i];
            if (item.AttributeType == t)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Get a vertex attribute array. Returns null if nothing is found. This is a reference value.
    /// </summary>
    public VertexAttributeArray? GetAttribute(int location)
    {
        if (extraAttributes == null)
            return null;

        if (location < 0 || location >= ExtraAttributeCount)
            return null;

        return extraAttributes[location];
    }

    /// <summary>
    /// Get a vertex attribute array. Returns null if nothing is found. This is a reference value.
    /// </summary>
    public T? GetAttribute<T>(int location) where T : VertexAttributeArray
    {
        if (extraAttributes == null)
            return null;

        if (location < 0 || location >= ExtraAttributeCount)
            return null;

        if (extraAttributes[location] is T vv)
            return vv;
        return null;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            Game.Main?.Window?.Graphics?.Delete(this);
            GC.SuppressFinalize(this);
            verticesHandle.Dispose();
        }
        disposed = true;
    }

    public MemoryHandle GetVerticesMemoryHandle() => verticesHandle;

    /// <summary>
    /// Returns the amount of extra attributes. The total amount of attributes equals this value + 3
    /// </summary>
    public int ExtraAttributeCount => extraAttributes?.Length ?? 0;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [DisallowNull]
    public IVertexDescriptor Descriptor { get; init; }
}

/// <summary>
/// Implements <see cref="VertexBuffer{TVertex}"/> with the default <see cref="Vertex"/> struct
/// </summary>
public class VertexBuffer : VertexBuffer<Vertex>
{
    public VertexBuffer()
    {
    }

    public VertexBuffer(Vertex[] vertices) : base(vertices, new Vertex.Descriptor())
    {
    }

    public VertexBuffer(Vertex[] vertices, uint[] indices, VertexAttributeArray[]? extraAttributes = null) : base(vertices, indices, new Vertex.Descriptor(), extraAttributes)
    {
    }
}
