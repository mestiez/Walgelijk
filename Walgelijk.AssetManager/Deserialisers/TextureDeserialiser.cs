namespace Walgelijk.AssetManager.Deserialisers;

public class TextureDeserialiser : IAssetDeserialiser<Texture>
{
    public Texture Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
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

///// <summary>
///// Deserialiser for sound objects, which 
///// </summary>
//public class SoundDeserialiser : IAssetDeserialiser<Sound>
//{

//}