using System.Text;

namespace Walgelijk.AssetManager;

public class TextDeserialiser : IAssetDeserialiser
{
    Type IAssetDeserialiser.ReturningType => typeof(string);

    bool IAssetDeserialiser.IsCandidate(in AssetMetadata assetMetadata) => true;

    object IAssetDeserialiser.Deserialise(Lazy<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var s = stream.Value;
        using var reader = new StreamReader(s, encoding: Encoding.UTF8, leaveOpen: false);
        return reader.ReadToEnd();
    }
}
