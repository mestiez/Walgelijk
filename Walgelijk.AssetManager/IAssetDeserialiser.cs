using System.Security.Cryptography;
using Walgelijk;

namespace Walgelijk.AssetManager;

public interface IAssetDeserialiser
{
    bool IsCandidate(in AssetMetadata assetMetadata);

    Type ReturningType { get; }

    object Deserialise(Stream stream, in AssetMetadata assetMetadata);
}
