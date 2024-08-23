using System.Linq;

namespace Walgelijk;

public static class VertexDescriptorExtensions
{
    /// <summary>
    /// Get the total size of the vertex object
    /// </summary>
    public static int GetTotalStride(this IVertexDescriptor descriptor)
        => descriptor.GetAttributes().Sum(static d => d.TotalSize);
}
