using System;
using System.IO;

namespace Walgelijk.Mock;

/// <summary>
/// Empty graphics implementation for testing purposes
/// </summary>
public class MockGraphics : IGraphics
{
    public DrawBounds DrawBounds { get; set; }
    public RenderTarget CurrentTarget { get; set; } = new MockRenderTarget();
    public StencilState Stencil { get; set; }

    public void Blit(RenderTexture source, RenderTexture destination) { }

    public void Clear(Color color) { }

    public void Delete(object obj) { }

    public void Delete<TVertex, TDescriptor>(VertexBuffer<TVertex, TDescriptor> graphicsObject)
        where TVertex : struct
        where TDescriptor : IVertexDescriptor<TVertex>, new()
    { }

    public void Draw<TVertex, TDescriptor>(VertexBuffer<TVertex, TDescriptor> vertexBuffer, Material? material = null)
        where TVertex : struct
        where TDescriptor : IVertexDescriptor<TVertex>, new()
    { }

    public void DrawInstanced<TVertex, TDescriptor>(VertexBuffer<TVertex, TDescriptor> vertexBuffer, int instanceCount, Material? material = null)
        where TVertex : struct
        where TDescriptor : IVertexDescriptor<TVertex>, new()
    { }

    public void SaveTexture(FileStream output, IReadableTexture texture) { }

    public void SetUniform<T>(Material material, string uniformName, T data) { }

    public bool TryGetId(RenderTexture graphicsObject, out int frameBufferId, out int[] textureId)
    {
        frameBufferId = 0;
        textureId = Array.Empty<int>();
        return false;
    }

    public bool TryGetId(IReadableTexture graphicsObject, out int textureId)
    {
        textureId = 0;
        return false;
    }

    public bool TryGetId(Material graphicsObject, out int id)
    {
        id = 0;
        return false;
    }

    public int TryGetId<TVertex, TDescriptor>(VertexBuffer<TVertex, TDescriptor> graphicsObject, out int vertexBufferId, out int indexBufferId, out int vertexArrayId, ref int[] vertexAttributeIds)
        where TVertex : struct
        where TDescriptor : IVertexDescriptor<TVertex>, new()
    {
        vertexBufferId = 0;
        indexBufferId = 0;
        vertexArrayId = 0;
        return -1;
    }

    public void Upload(object obj) { }

    public void Upload<TVertex, TDescriptor>(VertexBuffer<TVertex, TDescriptor> graphicsObject)
        where TVertex : struct
        where TDescriptor : IVertexDescriptor<TVertex>, new()
    { }
}
