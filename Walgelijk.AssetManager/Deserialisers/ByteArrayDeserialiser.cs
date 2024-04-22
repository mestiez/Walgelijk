using System.Text;

namespace Walgelijk.AssetManager.Deserialisers;

public class ByteArrayDeserialiser : IAssetDeserialiser<byte[]>
{
    public bool IsCandidate(in AssetMetadata assetMetadata) => true;

    public byte[] Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var s = stream();
        var b = new byte[assetMetadata.Size];
        var i = s.Read(b);
        if (i != assetMetadata.Size)
            Array.Resize(ref b, i);
        return b;
    }
}
