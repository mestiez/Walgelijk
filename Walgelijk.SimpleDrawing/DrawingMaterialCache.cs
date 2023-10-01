namespace Walgelijk.SimpleDrawing
{
    /// <summary>
    /// Caches textures bound to materials
    /// </summary>
    public class DrawingMaterialCache : Cache<IReadableTexture, Material>
    {
        protected override Material CreateNew(IReadableTexture raw) => DrawingMaterialCreator.Create(raw);

        protected override void DisposeOf(Material loaded)
        {
            loaded.Dispose();
        }
    }
}