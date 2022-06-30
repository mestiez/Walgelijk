using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Walgelijk;

/// <summary>
/// Global ID based resource directory
/// </summary>
public static class Assets
{
    private static readonly ConcurrentDictionary<Asset, AssetProvider> registeredAssets = new();
    private static readonly ConcurrentDictionary<Asset, object> loadedAssets = new();
    internal static readonly ConcurrentDictionary<Asset, string> NameByAsset = new();

    public delegate object AssetProvider(Asset asset);

    /// <summary>
    /// Loads the given asset
    /// </summary>
    public static T Load<T>(in Asset id)
    {
        if (TryLoad(id, out T? asset))
            return asset;

        throw new AssetException(id, "Failed to load asset");
    }

    /// <summary>
    /// Tries to load the given asset. Returns true if successful, meaning the loaded object is non-null
    /// </summary>
    public static bool TryLoad<T>(in Asset id, [NotNullWhen(true)] out T? asset)
    {
        if (!registeredAssets.TryGetValue(id, out var provider))
        {
            Logger.Error($"Attempt to load unregistered asset '{GetAssetName(id)}'");
            asset = default;
            return false;
        }

        if (loadedAssets.TryGetValue(id, out var val))
        {
            if (val is T typed)
            {
                asset = typed;
                return true;
            }

            Logger.Error($"Invalid attempt to load asset '{GetAssetName(id)}' as a {typeof(T).Name}");
            asset = default;
            return false;
        }

        var obj = provider(id);
        if (obj == null)
        {
            Logger.Error($"Asset '{GetAssetName(id)}' provided by {provider} is null!");
            asset = default;
            return false;
        }

        if (obj is not T t2)
        {
            Logger.Error($"Asset '{GetAssetName(id)}' provided by {provider} is not of the requested type {typeof(T)}!");
            asset = default;
            return false;
        }

        if (!loadedAssets.TryAdd(id, obj))
            throw new AssetException(id, "Asset has already been loaded, somehow");

        asset = t2;
        Logger.Log($"Asset {GetAssetName(id)} loaded");
        return true;
    }

    /// <summary>
    /// Gets the associated name or integer ID of the given asset
    /// </summary>
    public static string GetAssetName(in Asset asset)
    {
        if (NameByAsset.TryGetValue(asset, out var name))
            return name;
        return asset.Id.ToString();
    }

    /// <summary>
    /// Register an asset to the registry. This asset can later be loaded and retrieved using the given asset ID
    /// </summary>
    public static bool Register(in Asset id, AssetProvider provider)
    {
        if (registeredAssets.ContainsKey(id))
        {
            Logger.Error($"Asset ID collision for {GetAssetName(id)}");
            return false;
        }

        if (registeredAssets.TryAdd(id, provider))
        {
            Logger.Log($"Asset {GetAssetName(id)} registered ({provider})");
            return true;
        }
        return false;
    }

    public static async Task<T> LoadAsync<T>(Asset id)
    {
        return await Task.Run(() => Load<T>(id));
    }

    /// <summary>
    /// Register the contents of a directory to the asset system, using the file names with extension as their ID
    /// </summary>
    public static void RegisterEntireDirectory(AssetProvider provider, string dirPath, string pattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        foreach (var path in Directory.EnumerateFiles(dirPath, pattern, searchOption))
        {
            var name = Path.GetFileName(path);
            Register(name, provider);
        }
    }

    /// <summary>
    /// Standard path-based <see cref="Texture"/> loader
    /// </summary>
    public static object TextureFileProvider(Asset asset) => Resources.Load<Texture>(GetAssetName(asset), true);
    /// <summary>
    /// Standard path-based <see cref="AudioData"/> loader
    /// </summary>
    public static object AudioDataFileProvider(Asset asset) => Resources.Load<AudioData>(GetAssetName(asset), true);
}
