namespace Walgelijk.OpenTK
{
    internal struct GPUObjects
    {
        internal static readonly MaterialCache MaterialCache = new();
        internal static readonly TextureCache TextureCache = new();
        internal static readonly ShaderCache ShaderCache = new();
        internal static readonly MaterialTextureCache MaterialTextureCache = new();

        internal static readonly VertexBufferCache VertexBufferCache = new();
        internal static readonly RenderTextureCache RenderTextureCache = new();

        internal static readonly RenderTargetDictionary RenderTargetDictionary = new();
    }
}
