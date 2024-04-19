namespace Walgelijk.AssetManager;

public readonly struct AssetWrapper<T> : IDisposable
{
    public readonly GlobalAssetId Id;
    public readonly T Value;

    public AssetWrapper(GlobalAssetId id, T value)
    {
        Id = id;
        Value = value;
    }

    public void Dispose()
    {
        Assets.DisposeOf(Id);
    }
}