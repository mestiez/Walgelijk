using i3arnon.ConcurrentCollections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

/* Je moet detecteren of een asset een andere asset nodig heeft en als dat zo is moet je die eerder laden
 * Dit is nodig omdat er een lock is waardoor je niet assets kan laden terwijl je een asset laadt
 */

namespace Walgelijk.AssetManager;

public static class Assets
{
    public static IEnumerable<PackageId> AssetPackages => packageRegistry.Keys;
    public static readonly Hook OnAssetPackageRegistered = new();
    public static readonly Hook OnAssetPackageDeregistered = new();
    /// <summary>
    /// Provides a way to load assets in the background without having to worry about tasks, async, etc.
    /// </summary>
    public static AsyncAssetsInterface Async { get; } = new();

    private static readonly List<PackageId> sortedPackages = [];
    private static readonly ConcurrentDictionary<PackageId, AssetPackage> packageRegistry = [];
    private static readonly ConcurrentDictionary<GlobalAssetId, ConcurrentHashSet<IDisposable>> disposableChain = [];

    private static readonly ReaderWriterLockSlim enumerationLock = new(LockRecursionPolicy.SupportsRecursion);

    static Assets()
    {
        Resources.RegisterType(typeof(AssetPackage), AssetPackage.Load);
    }

    public static AssetPackage RegisterPackage(string path)
    {
        enumerationLock.EnterReadLock();
        lock (sortedPackages)
            try
            {
                var assetPackage = Resources.Load<AssetPackage>(path, true);
                if (!packageRegistry.TryAdd(assetPackage.Metadata.Id, assetPackage))
                {
                    //Resources.Unload(assetPackage);
                    // I am not sure if we should unload the package from the resource cache
                    // because it's possible that it was loaded succesfully previously
                    // TODO maybe check if it was already loaded, and only unload if not?
                    throw new Exception($"Package with id {assetPackage.Metadata.Id} already exists");
                }

                sortedPackages.Insert(0, assetPackage.Metadata.Id);

                OnAssetPackageRegistered.Dispatch();

                return assetPackage;
            }
            finally
            {
                enumerationLock.ExitReadLock();
            }
    }

    public static AssetPackage RegisterPackage(AssetPackage assetPackage)
    {
        enumerationLock.EnterReadLock();
        lock (sortedPackages)
            try
            {
                if (!packageRegistry.TryAdd(assetPackage.Metadata.Id, assetPackage))
                {
                    throw new Exception($"Package with id {assetPackage.Metadata.Id} already exists");
                }

                sortedPackages.Insert(0, assetPackage.Metadata.Id);
                OnAssetPackageRegistered.Dispatch();

                return assetPackage;
            }
            finally
            {
                enumerationLock.ExitReadLock();
            }
    }

    public static void ClearRegistry()
    {
        enumerationLock.EnterWriteLock();
        lock (sortedPackages)
            try
            {
                OnAssetPackageRegistered.Dispatch();
                packageRegistry.Clear();
                sortedPackages.Clear();
            }
            finally
            {
                enumerationLock.ExitWriteLock();
            }
    }

    public static AssetPackage GetPackage(PackageId id) => packageRegistry[id];

    public static bool TryGetPackage(PackageId id, [NotNullWhen(true)] out AssetPackage? assetPackage)
    {
        enumerationLock.EnterReadLock();
        try
        {
            return packageRegistry.TryGetValue(id, out assetPackage);
        }
        finally
        {
            enumerationLock.ExitReadLock();
        }
    }

    public static bool TryGetPackage(in ReadOnlySpan<char> id, [NotNullWhen(true)] out AssetPackage? assetPackage)
        => TryGetPackage(new(id), out assetPackage);

    public static bool UnloadPackage(PackageId id)
    {
        enumerationLock.EnterWriteLock();
        lock (sortedPackages)
            try
            {
                if (TryGetPackage(id, out var p) && packageRegistry.Remove(id, out _) && sortedPackages.Remove(id))
                {
                    OnAssetPackageDeregistered.Dispatch();
                    return true;
                }
                else
                    return false;
            }
            finally
            {
                enumerationLock.ExitWriteLock();
            }
    }

    public static bool UnloadPackage(in ReadOnlySpan<char> id) => UnloadPackage(new(id));

    /// <summary>
    /// Identical to <code>AssignLifetime(id, SceneLifetimeOperator.Shared)</code>
    /// </summary>
    public static void AssignSceneLifetime(GlobalAssetId id) => AssignLifetime(id, SceneLifetimeOperator.Shared);

    /// <summary>
    /// Assign a lifetime to an asset.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="lifetimeOperator"></param>
    public static void AssignLifetime(GlobalAssetId id, ILifetimeOperator lifetimeOperator)
    {
        Validate(ref id);

        lifetimeOperator.Triggered.AddListener(() =>
        {
            DisposeOf(id);
        });
    }

    /// <summary>
    /// Link an <see cref="IDisposable"/> instance to an asset, 
    /// so that when the asset is disposed, 
    /// the specified <see cref="IDisposable"/> is disposed as well
    /// </summary>
    public static void LinkDisposal(GlobalAssetId id, IDisposable d)
    {
        Validate(ref id);

        var set = disposableChain.Ensure(id);
        set.Add(d);
    }

    /// <summary>
    /// Undoes the linking by <see cref="LinkDisposal(GlobalAssetId, IDisposable)"/>
    /// </summary>
    public static void UnlinkDisposal(GlobalAssetId id, IDisposable d)
    {
        Validate(ref id);

        var set = disposableChain.Ensure(id);
        set.TryRemove(d);
    }

    /// <summary>
    /// Dispose the asset
    /// </summary>
    /// <param name="id"></param>
    public static void DisposeOf(GlobalAssetId id)
    {
        Validate(ref id);

        if (disposableChain.TryGetValue(id, out var set))
            foreach (var d in set)
            {
                try
                {
                    d.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Error($"Disposal chain error while disposing {id}: {e}");
                }
            }

        if (packageRegistry.TryGetValue(id.External, out var assetPackage))
        {
            assetPackage.DisposeOf(id.Internal);
            Logger.Log($"Disposed asset {id}");
        }
    }

    public static bool HasAsset(GlobalAssetId id)
    {
        if (TryValidate(ref id) && packageRegistry.TryGetValue(id.External, out var p))
            return p.HasAsset(id.Internal);
        return false;
    }

    public static bool HasAsset(AssetId id)
    {
        foreach (var item in packageRegistry.Values)
            if (item.HasAsset(id))
                return true;
        return false;
    }

    public static bool TryLoad<T>(AssetId assetId, out AssetRef<T> assetRef)
    {
        return TryLoad(FindFirst(assetId), out assetRef);
    }

    public static bool TryLoad<T>(GlobalAssetId id, out AssetRef<T> assetRef)
    {
        if (TryValidate(ref id) && packageRegistry.TryGetValue(id.External, out var assetPackage))
        {
            if (assetPackage.HasAsset(id.Internal))
            {
                assetRef = new AssetRef<T>(id);
                return true;
            }
        }

        assetRef = AssetRef<T>.None;
        return false;
    }

    /// <summary>
    /// Load the asset with the given ID. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static AssetRef<T> Load<T>(GlobalAssetId id)
    {
        Validate(ref id);

        if (packageRegistry.TryGetValue(id.External, out var assetPackage))
            return new AssetRef<T>(id);
        throw new KeyNotFoundException($"Asset package {id.External} not found");
    }

    /// <summary>
    /// Load the asset with the given ID. Walks through each registered asset package in <see cref="AssetPackages"/> in reverse order (LIFO) and returns the first find.
    /// </summary>
    public static AssetRef<T> Load<T>(AssetId assetId) => Load<T>(FindFirst(assetId));

    /// <summary>
    /// Directly load the data without caching it. 
    /// Use with caution, as the resulting object will be disconnected from the asset manager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T LoadNoCache<T>(GlobalAssetId id)
    {
        Validate(ref id);

        if (packageRegistry.TryGetValue(id.External, out var assetPackage))
            return assetPackage.LoadNoCache<T>(id.Internal);

        throw new KeyNotFoundException($"Asset package {id.External} not found");
    }

    /// <summary>
    /// Directly load the data without caching it. 
    /// Use with caution, as the resulting object will be disconnected from the asset manager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T LoadNoCache<T>(AssetId assetId) => LoadNoCache<T>(FindFirst(assetId));

    public static AssetMetadata GetMetadata(GlobalAssetId id)
    {
        Validate(ref id);

        if (TryGetMetadata(id, out var m))
            return m;

        throw new KeyNotFoundException($"Asset package {id.External} not found");
    }

    public static AssetMetadata GetMetadata(AssetId assetId) => GetMetadata(FindFirst(assetId));

    public static bool TryGetMetadata(GlobalAssetId id, out AssetMetadata metadata)
    {
        if (TryValidate(ref id) && packageRegistry.TryGetValue(id.External, out var assetPackage)
            && assetPackage.HasAsset(id.Internal))
        {
            metadata = assetPackage.GetMetadata(id.Internal);
            return true;
        }

        metadata = default;
        return false;
    }

    public static bool TryGetMetadata(AssetId assetId, out AssetMetadata metadata)
    {
        if (TryFindFirst(assetId, out var globalId))
            return TryGetMetadata(globalId, out metadata);

        metadata = default;
        return false;
    }

    /// <summary>
    /// Enumerates the folder for every package and returns all assets found
    /// </summary>
    public static IEnumerable<GlobalAssetId> EnumerateFolder(string folder, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        foreach (var p in packageRegistry.Values)
            foreach (var a in p.EnumerateFolder(folder, searchOption))
                yield return new GlobalAssetId(p.Metadata.Id, a);
    }

    public static IEnumerable<GlobalAssetId> GetAllAssets()
    {
        foreach (var p in packageRegistry.Values)
            foreach (var a in p.All)
                yield return new(p.Metadata.Id, a);
    }

    public static IEnumerable<GlobalAssetId> QueryTags(string tag)
    {
        foreach (var p in packageRegistry.Values)
            foreach (var a in p.QueryTags(tag))
                yield return new GlobalAssetId(p.Metadata.Id, a);
    }

    public static GlobalAssetId FindFirst(AssetId assetId)
    {
        if (TryFindFirst(assetId, out var global))
            return global;
        throw new Exception($"No package found with asset {assetId}");
    }

    public static bool TryFindFirst(AssetId assetId, out GlobalAssetId globalId)
    {
        lock (sortedPackages)
            foreach (var packageId in sortedPackages)
                if (packageRegistry[packageId].HasAsset(assetId))
                {
                    globalId = new GlobalAssetId(packageId, assetId);
                    return true;
                }

        globalId = default;
        return false;
    }

    internal static T LoadDirect<T>(GlobalAssetId id)
    {
        Validate(ref id);

        if (packageRegistry.TryGetValue(id.External, out var assetPackage))
            return assetPackage.Load<T>(id.Internal);
        throw new KeyNotFoundException($"Asset package {id.External} not found");
    }

    internal static T LoadDirect<T>(AssetId id) => LoadDirect<T>(FindFirst(id));

    public static bool IsCached(GlobalAssetId id)
    {
        if (!TryValidate(ref id))
            return false;

        if (packageRegistry.TryGetValue(id.External, out var assetPackage))
            return assetPackage.IsCached(id.Internal);

        return false;
    }

    /// <summary>
    /// Resolve agnosticism and throw if not found
    /// </summary>
    internal static void Validate(ref GlobalAssetId id)
    {
        if (id.IsAgnostic)
            id = id.ResolveExternal();

        if (id.External == PackageId.None)
            throw new Exception($"External ID is none, but asset not found in any package: {id.External}:{id.Internal}");
    }

    /// <summary>
    /// Resolve agnosticism and throw if not found
    /// </summary>
    internal static bool TryValidate(ref GlobalAssetId id)
    {
        if (id.IsAgnostic)
            id = id.ResolveExternal();

        return id.External != PackageId.None;
    }
}
