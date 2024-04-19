namespace Walgelijk.AssetManager.Deserialisers;

public class TextureDeserialiser : IAssetDeserialiser
{
    public Type ReturningType => typeof(Texture);

    public object Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
    {
        using var b = stream();
        var buffer = new byte[assetMetadata.Size];
        b.Read(buffer);
        return TextureLoader.FromBytes(buffer);
    }

    public bool IsCandidate(in AssetMetadata assetMetadata)
    {
        var p = assetMetadata.Path;
        return TextureLoader.Decoders.Any(d => d.CanDecode(p));
    }
}