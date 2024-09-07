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

    public void Delete<TVertex>(VertexBuffer<TVertex> graphicsObject)
        where TVertex : struct
    { }

    public void Draw<TVertex>(VertexBuffer<TVertex> vertexBuffer, Material? material = null)
        where TVertex : struct
    { }

    public void DrawInstanced<TVertex>(VertexBuffer<TVertex> vertexBuffer, int instanceCount, Material? material = null)
        where TVertex : struct
    { }

    public Color SampleTexture(IReadableTexture tex, int x, int y)
    {
        return Utilities.RandomColour();
    }

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

    public int TryGetId<TVertex>(VertexBuffer<TVertex> graphicsObject, out int vertexBufferId, out int indexBufferId, out int vertexArrayId, ref int[] vertexAttributeIds)
        where TVertex : struct
    {
        vertexBufferId = 0;
        indexBufferId = 0;
        vertexArrayId = 0;
        return -1;
    }

    public void Upload(object obj) { }

    public void Upload<TVertex>(VertexBuffer<TVertex> graphicsObject)
        where TVertex : struct
    { }
}
