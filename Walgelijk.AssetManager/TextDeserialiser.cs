using System.Text;

namespace Walgelijk.AssetManager;

// example deserialiser
public class TextDeserialiser : IAssetDeserialiser
{
    Type IAssetDeserialiser.ReturningType => typeof(string);

    bool IAssetDeserialiser.IsCandidate(in AssetMetadata assetMetadata)
        => assetMetadata.MimeType.StartsWith("text");

    object IAssetDeserialiser.Deserialise(Stream stream, in AssetMetadata assetMetadata)
    {
        using var reader = new StreamReader(stream, encoding: Encoding.UTF8, leaveOpen: false);
        return reader.ReadToEnd();
    }
}

