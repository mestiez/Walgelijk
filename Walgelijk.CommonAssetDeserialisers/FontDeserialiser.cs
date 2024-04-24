using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;

namespace Walgelijk.CommonAssetDeserialisers;

public class FontDeserialiser : IAssetDeserialiser<Font>
{
    public Font Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var s = stream();
        var wfont = FontLoader.LoadWf(s);
        return wfont;
    }

    public bool IsCandidate(in AssetMetadata m)
        => m.MimeType.Equals("font/walgelijk", StringComparison.InvariantCultureIgnoreCase);
}
