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

    public void Blit(RenderTexture source, RenderTexture destination) { }

    public void Clear(Color color) { }

    public void Delete(object obj) { }

    public void Draw(VertexBuffer vertexBuffer, Material? material = null) { }

    public void DrawInstanced(VertexBuffer vertexBuffer, int instanceCount, Material? material = null) { }

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

    public int TryGetId(VertexBuffer graphicsObject, out int vertexBufferId, out int indexBufferId, out int vertexArrayId, ref int[] vertexAttributeIds)
    {
        vertexBufferId = 0;
        indexBufferId = 0;
        vertexArrayId = 0;
        return -1;
    }

    public bool TryGetId(Material graphicsObject, out int id)
    {
        id = 0;
        return false;
    }

    public void Upload(object obj) { }
}
