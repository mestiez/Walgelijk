namespace Walgelijk.OpenTK;

internal static class GPUObjects
{
    internal static readonly MaterialCache MaterialCache = new();
    internal static readonly TextureCache TextureCache = new();
    internal static readonly ShaderCache ShaderCache = new();
    internal static readonly MaterialTextureCache MaterialTextureCache = new();

    internal static readonly VertexBufferCacheCache VertexBufferCache = new();
    internal static readonly RenderTextureCache RenderTextureCache = new();

    internal static readonly RenderTargetDictionary RenderTargetDictionary = new();
}
