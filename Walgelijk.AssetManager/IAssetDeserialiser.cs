namespace Walgelijk.AssetManager;

public interface IAssetDeserialiser
{
    bool IsCandidate(in AssetMetadata assetMetadata);

    Type ReturningType { get; }

    object Deserialise(Lazy<Stream> stream, in AssetMetadata assetMetadata);
}
