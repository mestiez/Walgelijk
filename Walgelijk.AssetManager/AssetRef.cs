namespace Walgelijk.AssetManager;

public readonly struct AssetRef<T> : IDisposable
{
    public readonly GlobalAssetId Id;
    public T Value => Assets.LoadDirect<T>(Id);

    public AssetRef(GlobalAssetId id)
    {
        Id = id;
    }

    public void Dispose()
    {
        Assets.DisposeOf(Id);
    }

    public static implicit operator T(AssetRef<T> t) => t.Value;
}