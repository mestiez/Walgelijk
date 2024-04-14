namespace Walgelijk.AssetManager;

public static class AssetDeserialisers
{
    private static HashSet<IAssetDeserialiser> Deserialisers = [];

    private static Dictionary<Type, List<IAssetDeserialiser>> byType = [];

    static AssetDeserialisers()
    {
        Register(new TextDeserialiser());
    }

    public static void Register(IAssetDeserialiser d)
    {
        Deserialisers.Add(d);
        var set = byType.Ensure(d.ReturningType);
        set.Add(d);
    }

    public static T Load<T>(in Asset asset)
    {
        if (byType.TryGetValue(typeof(T), out var set))
        {
            var metadata = asset.Metadata;
            var candidate = set.FirstOrDefault(d => d.IsCandidate(metadata));
            if (candidate != null)
            {
                using var stream = asset.Stream.Value;
                var result = candidate.Deserialise(stream, metadata);
                if (result is T typed)
                    return typed;
                else
                {
                    if (result is IDisposable d)
                        d.Dispose();

                    throw new Exception(
                        $"Asset {asset.Metadata.Id} at \"{asset.Metadata.Path}\" cannot be deserialised as {typeof(T)}. " +
                        $"Attempted to use best candidate {candidate.GetType().FullName}."
                        );
                }
            }
        }

        throw new Exception($"Asset {asset.Metadata.Id} at \"{asset.Metadata.Path}\" cannot be deserialised as {typeof(T)}. No candidates found.");
    }
}
