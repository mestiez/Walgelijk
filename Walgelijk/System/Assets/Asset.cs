namespace Walgelijk;

/// <summary>
/// An asset ID. This is actually just a signed 32 bit integer.
/// </summary>
public readonly struct Asset
{
    /// <summary>
    /// The unique identity
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// Construct an asset ID from a string. The actual Id is just the hashcode of the string. 
    /// <br></br>
    /// Note that this constructor adds permanently binds the asset ID and the given string in a static dictionary, accessible from <see cref="Assets.GetAssetName(in Asset)"/>
    /// </summary>
    public Asset(in string id)
    {
        Id = id.GetHashCode();
        Assets.NameByAsset.TryAdd(this, id);
    }

    /// <summary>
    /// Construct an asset ID from an integer
    /// </summary>
    public Asset(int id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Asset identity &&
               Id == identity.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public override string? ToString() => Id.ToString();

    public static bool operator ==(Asset left, Asset right)
    {
        return left.Id.Equals(right.Id);
    }

    public static bool operator !=(Asset left, Asset right)
    {
        return !(left == right);
    }

    public static implicit operator Asset(in string id)
    {
        return new Asset(id);
    }

    public static implicit operator Asset(in int id)
    {
        return new Asset(id);
    }

    public static implicit operator int(in Asset id)
    {
        return id.Id;
    }
}
