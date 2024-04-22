using System.Text;

namespace Walgelijk.AssetManager.Deserialisers;

public class StringDeserialiser : IAssetDeserialiser<string>
{
    public bool IsCandidate(in AssetMetadata assetMetadata) => true;

    public string Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var s = stream();
        using var reader = new StreamReader(s, encoding: Encoding.UTF8, leaveOpen: false);
        return reader.ReadToEnd();
    }
}
