using System.Linq;

namespace Walgelijk;

public static class VertexDescriptorExtensions
{
    /// <summary>
    /// Get the total size of the vertex object
    /// </summary>
    public static int GetTotalStride<T>(this IVertexDescriptor<T> descriptor) 
        => descriptor.GetAttributes().Sum(static d => d.TotalSize);

    public static TDescriptor GetDescriptor<TVertex, TDescriptor>(this VertexBuffer<TVertex, TDescriptor> v) where TDescriptor : IVertexDescriptor<TVertex>, new() where TVertex : struct 
        => new TDescriptor();
}
