using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Walgelijk.AssetManager;

public static class Assets
{
    private static readonly ConcurrentDictionary<int, AssetPackage> packageRegistry = [];
    private static readonly SemaphoreSlim enumerationLock = new(1);

    private static bool isDisposingPackage = false;

    static Assets()
    {
        Resources.RegisterType(typeof(AssetPackage), AssetPackage.Load);
    }

    public static AssetPackage RegisterPackage(string path)
    {
        enumerationLock.Wait();
        try
        {
            var assetPackage = Resources.Load<AssetPackage>(path, true);
            var numerical = Hashes.MurmurHash1(assetPackage.Metadata.Id);
            if (!packageRegistry.TryAdd(numerical, assetPackage))
            {
                Resources.Unload(assetPackage);
                throw new Exception($"Package with id {numerical} already exists");
            }
            return assetPackage;
        }
        finally
        {
            enumerationLock.Release();
        }
    }

    public static IEnumerable<int> AssetPackages => packageRegistry.Keys;

    public static void ClearRegistry()
    {
        if (isDisposingPackage)
            throw new InvalidOperationException("The package registry cannot be cleared while disposing a package");

        enumerationLock.Wait();
        try
        {
            var keys = new Stack<int>(AssetPackages);
            while (keys.TryPop(out var id))
            {
                if (packageRegistry.TryGetValue(id, out var p))
                {
                    Resources.Unload(p);
                    p.Dispose();
                }
            }
            packageRegistry.Clear();
        }
        finally
        {
            enumerationLock.Release();
        }
    }

    public static bool TryGetPackage(int id, [NotNullWhen(true)] out AssetPackage? assetPackage)
    {
        if (isDisposingPackage)
            throw new InvalidOperationException("Packages cannot be retrieved while disposing a package");

        enumerationLock.Wait();
        try
        {
            return packageRegistry.TryGetValue(id, out assetPackage);
        }
        finally
        {
            enumerationLock.Release();
        }
    }

    public static bool TryGetPackage(ReadOnlySpan<char> id, [NotNullWhen(true)] out AssetPackage? assetPackage)
        => TryGetPackage(Hashes.MurmurHash1(id), out assetPackage);

    public static bool UnloadPackage(int id)
    {
        enumerationLock.Wait();
        try
        {
            if (TryGetPackage(id, out var p))
            {
                isDisposingPackage = true;
                Resources.Unload(p);
                p.Dispose();
                return true;
            }
            else
                return false;
        }
        finally
        {
            isDisposingPackage = false;
            enumerationLock.Release();
        }
    }

    public static bool UnloadPackage(ReadOnlySpan<char> id)
        => UnloadPackage(Hashes.MurmurHash1(id));

    public static void AssignLifetime(GlobalAssetId id, ILifetimeOperator lifetimeOperator)
    {
        lifetimeOperator.Triggered.AddListener(() => DisposeOf(id));
    }

    public static void DisposeOf(in GlobalAssetId id)
    {
        if (packageRegistry.TryGetValue(id.External, out var assetPackage))
            assetPackage.DisposeOf(id.Internal);
    }

    public static AssetWrapper<T> Load<T>(in ReadOnlySpan<char> id) => Load<T>(new GlobalAssetId(id));

    public static AssetWrapper<T> Load<T>(in GlobalAssetId id)
    {
        enumerationLock.Wait();
        try
        {
            if (packageRegistry.TryGetValue(id.External, out var assetPackage))
            {
                var asset = assetPackage.Load<T>(id.Internal);
                return new AssetWrapper<T>(id, asset);
            }
            throw new Exception($"Asset package {id.External} not found");
        }
        finally
        {
            enumerationLock.Release();
        }
    }
}

public readonly struct AssetWrapper<T> : IDisposable
{
    public readonly GlobalAssetId Id;
    public readonly T Value;

    public AssetWrapper(GlobalAssetId id, T value)
    {
        Id = id;
        Value = value;
    }

    public void Dispose()
    {
        Assets.DisposeOf(Id);
    }
}