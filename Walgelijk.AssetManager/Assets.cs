﻿using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    private static readonly List<PackageId> sortedPackages = [];
    private static readonly ConcurrentDictionary<PackageId, AssetPackage> packageRegistry = [];
    private static readonly ConcurrentDictionary<GlobalAssetId, ConcurrentBag<IDisposable>> disposableChain = [];
    private static readonly ConcurrentDictionary<GlobalAssetId, GlobalAssetId> replacementTable = [];

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

    public static void AssignLifetime(GlobalAssetId id, ILifetimeOperator lifetimeOperator)
    {
        if (id == GlobalAssetId.None)
            throw new Exception("Id is None");

        lifetimeOperator.Triggered.AddListener(() =>
        {
            DisposeOf(id);
        });
    }

    /// <summary>
    /// Link an <see cref="IDisposable"/> instance to an asset, so that when the asset is disposed, the specified <see cref="IDisposable"/> is disposed as well
    /// </summary>
    public static void LinkDisposal(in GlobalAssetId id, IDisposable d)
    {
        var set = disposableChain.Ensure(id);
        set.Add(d);
    }

    /// <summary>
    /// Add an entry to the replacement table. Note how this will override the existing replacement.
    /// Note how this will dispose of the original asset.
    /// </summary>
    /// <param name="original">The original asset ID - the one that is to be replaced</param>
    /// <param name="replacement">The substitute ID</param>
    public static void SetReplacement(in GlobalAssetId original, in GlobalAssetId replacement)
    {
        if (original.External == PackageId.None)
            throw new Exception("Original package ID is None");

        if (replacement.External == PackageId.None)
            throw new Exception("Replacement package ID is None");

        replacementTable.AddOrSet(original, replacement);
        DisposeOf(original); // we have to dispose the original to force a reload
    }

    /// <summary>
    /// Clear the set replacement for the given original ID. 
    /// If any replacements were set using <see cref="SetReplacement(GlobalAssetId, GlobalAssetId)"/>, 
    /// they will be undone and the asset will refer to itself again.
    /// Note how this will dispose of the original asset.
    /// </summary>
    public static void ClearReplacement(in GlobalAssetId original)
    {
        if (original.External == PackageId.None)
            throw new Exception("Id is None");

        replacementTable.TryRemove(original, out _);
        DisposeOf(original);
    }

    /// <summary>
    /// Clear all set replacements.
    /// </summary>
    public static void ClearReplacements()
    {
        var keys = replacementTable.Keys.ToArray();
        replacementTable.Clear();
        foreach (var k in keys)
            DisposeOf(k);
    }

    public static bool HasReplacement(in GlobalAssetId original) => replacementTable.ContainsKey(original);

    /// <summary>
    /// If the given ID is remapped in the <see cref="replacementTable"/>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static GlobalAssetId ApplyReplacement(in GlobalAssetId id)
    {
        if (id.External == PackageId.None)
            throw new Exception("Id is None");

        if (replacementTable.TryGetValue(id, out var replacement))
            return replacement;
        return id;
    }

    /// <summary>
    /// Dispose the asset
    /// </summary>
    /// <param name="id"></param>
    public static void DisposeOf(GlobalAssetId id)
    {
        if (id.External == PackageId.None)
            throw new Exception("Id is None");

        if (disposableChain.TryRemove(id, out var set))
            while (set.TryTake(out var d))
                try
                {
                    d.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Error($"Disposal chain error while disposing {id}: {e}");
                }

        if (packageRegistry.TryGetValue(id.External, out var assetPackage))
            assetPackage.DisposeOf(id.Internal);
    }

    public static bool HasAsset(in GlobalAssetId id)
    {
        if (packageRegistry.TryGetValue(id.External, out var p))
            return p.HasAsset(id.Internal);
        return false;
    }

    public static bool HasAsset(in AssetId id)
    {
        foreach (var item in packageRegistry.Values)
            if (item.HasAsset(id))
                return true;
        return false;
    }

    public static bool TryLoad<T>(in ReadOnlySpan<char> id, out AssetRef<T> assetRef)
    {
        if (id.Contains(':'))
            return TryLoad<T>(new GlobalAssetId(id), out assetRef);

        return TryLoad<T>(new AssetId(id), out assetRef);
    }

    public static bool TryLoad<T>(AssetId assetId, out AssetRef<T> assetRef)
    {
        lock (sortedPackages)
            foreach (var packageId in sortedPackages)
                if (packageRegistry[packageId].HasAsset(assetId))
                    return TryLoad<T>(new GlobalAssetId(packageId, assetId), out assetRef);

        assetRef = default;
        return false;
    }

    public static bool TryLoad<T>(GlobalAssetId id, out AssetRef<T> assetRef)
    {
        if (id.External == PackageId.None)
            throw new Exception("Id is None");

        var replacementId = ApplyReplacement(id);

        if (packageRegistry.TryGetValue(replacementId.External, out var assetPackage))
        {
            if (assetPackage.HasAsset(replacementId.Internal))
            {
                assetRef = new AssetRef<T>(id);
                return true;
            }
        }

        assetRef = AssetRef<T>.None;
        return false;
    }

    public static AssetRef<T> Load<T>(in ReadOnlySpan<char> id)
    {
        if (id.Contains(':'))
            return Load<T>(new GlobalAssetId(id));
        return Load<T>(new AssetId(id));
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
        if (id.External == PackageId.None)
            throw new Exception("Id is None");

        var replacementId = ApplyReplacement(id);

        if (packageRegistry.TryGetValue(replacementId.External, out var assetPackage))
            return new AssetRef<T>(id);
        throw new KeyNotFoundException($"Asset package {replacementId.External} not found");
    }

    /// <summary>
    /// Load the asset with the given ID. Walks through each registered asset package in <see cref="AssetPackages"/> in reverse order (LIFO) and returns the first find.
    /// </summary>
    public static AssetRef<T> Load<T>(AssetId assetId)
    {
        if (assetId == AssetId.None)
            throw new Exception("Id is None");

        lock (sortedPackages)
            foreach (var packageId in sortedPackages)
                if (packageRegistry[packageId].HasAsset(assetId))
                    return Load<T>(new GlobalAssetId(packageId, assetId));

        throw new KeyNotFoundException($"Asset {assetId} not found in any package");
    }

    /// <summary>
    /// Directly load the data without caching it. 
    /// Use with caution, as the resulting object will be disconnected from the asset manager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T LoadNoCache<T>(GlobalAssetId id)
    {
        if (id.External == PackageId.None)
            throw new Exception("Id is None");

        var replacementId = ApplyReplacement(id);

        if (packageRegistry.TryGetValue(replacementId.External, out var assetPackage))
            return assetPackage.LoadNoCache<T>(id.Internal);

        throw new KeyNotFoundException($"Asset package {replacementId.External} not found");
    }

    /// <summary>
    /// Directly load the data without caching it. 
    /// Use with caution, as the resulting object will be disconnected from the asset manager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T LoadNoCache<T>(AssetId assetId)
    {
        lock (sortedPackages)
            foreach (var packageId in sortedPackages)
                if (packageRegistry[packageId].HasAsset(assetId))
                    return LoadNoCache<T>(new GlobalAssetId(packageId, assetId));
        throw new KeyNotFoundException($"Asset {assetId} not found in any package");
    }

    public static AssetMetadata GetMetadata(GlobalAssetId id)
    {
        if (id.External == PackageId.None)
            throw new Exception("Id is None");

        if (TryGetMetadata(id, out var m))
            return m;

        throw new KeyNotFoundException($"Asset package {id.External} not found");
    }

    public static AssetMetadata GetMetadata(AssetId assetId)
    {
        lock (sortedPackages)
            foreach (var packageId in sortedPackages)
                if (packageRegistry[packageId].HasAsset(assetId))
                    return GetMetadata(new GlobalAssetId(packageId, assetId));
        throw new KeyNotFoundException($"Asset {assetId} not found in any package");
    }

    public static bool TryGetMetadata(GlobalAssetId id, out AssetMetadata metadata)
    {
        if (id.External == PackageId.None)
            throw new Exception("Id is None");

        var replacementId = ApplyReplacement(id);

        if (packageRegistry.TryGetValue(replacementId.External, out var assetPackage)
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
        lock (sortedPackages)
            foreach (var packageId in sortedPackages)
                if (packageRegistry[packageId].HasAsset(assetId))
                    return TryGetMetadata(new GlobalAssetId(packageId, assetId), out metadata);

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

    internal static T LoadDirect<T>(GlobalAssetId id)
    {
        var replacementId = ApplyReplacement(id);

        if (packageRegistry.TryGetValue(replacementId.External, out var assetPackage))
            return assetPackage.Load<T>(replacementId.Internal);
        throw new KeyNotFoundException($"Asset package {replacementId.External} not found");
    }
}
