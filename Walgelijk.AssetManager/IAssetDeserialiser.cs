namespace Walgelijk.AssetManager;

public interface IAssetDeserialiser
{
    bool IsCandidate(in AssetMetadata assetMetadata);

    Type ReturningType { get; }

    object Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata);
}
