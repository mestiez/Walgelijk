namespace Walgelijk.OpenTK
{
    internal struct GPUObjects
    {
        internal static readonly MaterialCache MaterialCache = new MaterialCache();
        internal static readonly TextureCache TextureCache = new TextureCache();
        internal static readonly MaterialTextureCache MaterialTextureCache = new MaterialTextureCache();

        internal static readonly VertexBufferCache VertexBufferCache = new VertexBufferCache();
        internal static readonly RenderTextureCache RenderTextureCache = new RenderTextureCache();

        internal static readonly RenderTargetDictionary RenderTargetDictionary = new RenderTargetDictionary();
    }
}
