using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Walgelijk;

/// <summary>
/// Global ID based resource directory. Uses <see cref="Resources"/> to load and store the actual data.
/// </summary>
public static class Assets
{
    private static readonly ConcurrentDictionary<Asset, FileInfo> registeredAssets = new();
    private static readonly ConcurrentDictionary<Asset, object> loadedAssets = new();
    internal static readonly ConcurrentDictionary<Asset, string> NameByAsset = new();

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
    /// Tries to load the given asset. Returns true if successful, meaning the loaded object is non-null and where <see cref="Resources.CanLoad(Type)"/> returns true for its type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="asset"></param>
    /// <returns></returns>
    public static bool TryLoad<T>(in Asset id, [NotNullWhen(true)] out T? asset)
    {
        if (!registeredAssets.TryGetValue(id, out var file))
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
            else
            {
                Logger.Error($"Invalid attempt to load asset '{GetAssetName(id)}' as a {typeof(T).Name}");
                asset = default;
                return false;
            }
        }

        var obj = Resources.Load<T>(file.FullName, true);
        if (obj == null)
        {

            Logger.Error($"Asset '{GetAssetName(id)}' at {file.FullName} is null!");
            asset = default;
            return false;
        }

        if (!loadedAssets.TryAdd(id, obj))
            throw new AssetException(id, "Asset has already been loaded, somehow");

        asset = obj;
        Logger.Log($"Asset {GetAssetName(id)} loaded");
        return true;
    }

    public static string GetAssetName(in Asset asset)
    {
        if (NameByAsset.TryGetValue(asset, out var name))
            return name;
        if (registeredAssets.TryGetValue(asset, out var file))
            return file.Name;
        return asset.Id.ToString();
    }

    /// <summary>
    /// Get file associated with the asset.
    /// </summary>
    public static FileInfo Get(in Asset asset) => registeredAssets[asset];

    /// <summary>
    /// Tries to get the file associated with the asset. Returns true if successful.
    /// </summary>
    public static bool TryGet(in Asset id, [NotNullWhen(true)] out FileInfo? asset) => registeredAssets.TryGetValue(id, out asset);

    /// <summary>
    /// Register an asset to the registry. This asset can later be loaded and retrieved using the given asset ID
    /// </summary>
    public static bool Register(string path, in Asset id)
    {
        if (registeredAssets.ContainsKey(id))
        {
            Logger.Error($"Asset ID collision for {GetAssetName(id)}");
            return false;
        }

        if (registeredAssets.TryAdd(id, new FileInfo(path)))
        {
            Logger.Log($"Asset {GetAssetName(id)} registered ({Path.GetFileName(path)})");
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
    public static void RegisterEntireDirectory(string dirPath, string pattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        foreach (var path in Directory.EnumerateFiles(dirPath, pattern, searchOption))
        {
            var name = Path.GetFileName(path);
            Register(path, new Asset(name));
        }
    }
}
