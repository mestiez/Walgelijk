namespace Walgelijk.AssetManager.Deserialisers;

public interface IAssetDeserialiser
{
    bool IsCandidate(in AssetMetadata assetMetadata);

    Type ReturningType { get; }

    object Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata);
}

public interface IAssetDeserialiser<T> : IAssetDeserialiser where T : notnull
{
    Type IAssetDeserialiser.ReturningType => typeof(T);

    object IAssetDeserialiser.Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata) 
        => Deserialise(stream, assetMetadata);

    new T Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata);
}
