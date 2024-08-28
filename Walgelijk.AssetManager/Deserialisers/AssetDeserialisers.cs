using System.Collections.Concurrent;

namespace Walgelijk.AssetManager.Deserialisers;

public static class AssetDeserialisers
{
    private readonly static HashSet<IAssetDeserialiser> deserialisers = [];
    private readonly static ConcurrentDictionary<Type, List<IAssetDeserialiser>> byType = [];
    private readonly static ManualResetEventSlim setLock = new(true);

    static AssetDeserialisers()
    {
        Register(new StringDeserialiser());
        Register(new ByteArrayDeserialiser());
        Register(new TextureDeserialiser());
    }

    public static void Register(IAssetDeserialiser d)
    {
        setLock.Wait();
        try
        {
            deserialisers.Add(d);
            var set = byType.Ensure(d.ReturningType);
            set.Add(d);
        }
        finally
        {
            setLock.Set();
        }
    }

    public static void Unregister(IAssetDeserialiser d)
    {
        setLock.Wait();
        try
        {
            if (deserialisers.Remove(d))
                foreach (var item in byType.Values)
                    item.Remove(d);
        }
        finally
        {
            setLock.Set();
        }
    }

    public static void Unregister<T>() where T : IAssetDeserialiser
    {
        setLock.Wait();
        try
        {
            if (deserialisers.RemoveWhere(d => d is T) > 0)
                foreach (var item in byType.Values)
                    item.RemoveAll(d => d is T);
        }
        finally
        {
            setLock.Set();
        }
    }

    public static void Clear()
    {
        setLock.Wait();
        try
        {
            deserialisers.Clear();
            byType.Clear();
        }
        finally
        {
            setLock.Set();
        }
    }

    public static bool HasCandidate<T>(in Asset asset)
    {
        setLock.Wait();

        try
        {
            if (!byType.TryGetValue(typeof(T), out var set))
                return false;

            var metadata = asset.Metadata;
            var candidate = set.FirstOrDefault(d => d.IsCandidate(metadata));
            if (candidate == null)
                return false;

            return true;
        }
        finally
        {
            setLock.Set();
        }
    }

    public static T Load<T>(in Asset asset)
    {
        setLock.Wait();
        try
        {
            if (byType.TryGetValue(typeof(T), out var set))
            {
                var metadata = asset.Metadata;
                var candidate = set.FirstOrDefault(d => d.IsCandidate(metadata));
                if (candidate != null)
                {
                    var result = candidate.Deserialise(asset.Stream, metadata);
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
        finally
        {
            setLock.Set();
        }
    }
}
