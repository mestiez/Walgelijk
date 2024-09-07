using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

[JsonConverter(typeof(AssetRefConverter))]
public readonly struct AssetRef<T> : IDisposable, IEquatable<AssetRef<T>>
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

    /// <summary>
    /// Returns false if the ID is none
    /// </summary>
    public bool IsValid => Id != GlobalAssetId.None;

    public static AssetRef<T> None => new AssetRef<T>(GlobalAssetId.None);

    public static implicit operator T(AssetRef<T> t) => t.Value;

    public static bool operator ==(AssetRef<T> left, AssetRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AssetRef<T> left, AssetRef<T> right)
    {
        return !(left == right);
    }

    public override string ToString() => Id.ToNamedString();

    public override bool Equals(object? obj)
    {
        return obj is AssetRef<T> @ref && Equals(@ref);
    }

    public bool Equals(AssetRef<T> other)
    {
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}