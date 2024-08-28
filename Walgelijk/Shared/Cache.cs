using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Walgelijk;

/// <summary>
/// A concurrent version of <see cref="Cache{UnloadedType, LoadedType}"/>. 
/// </summary>
/// <typeparam name="UnloadedType">The key. This object is usually light and cheap to create</typeparam>
/// <typeparam name="LoadedType">The loaded object. This object is usually heavy and expensive to create</typeparam>
public abstract class ConcurrentCache<UnloadedType, LoadedType> where UnloadedType : notnull
{
    protected readonly ConcurrentDictionary<UnloadedType, LoadedType> Loaded = [];
    protected readonly SemaphoreSlim LoadingNewLock = new(1);
    protected readonly SemaphoreSlim AccessReadCacheLock = new(1);

    /// <summary>
    /// Load or create a <typeparamref name="LoadedType"/> from an <typeparamref name="UnloadedType"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual LoadedType Load(UnloadedType obj)
    {
        using var @lock = new DeferredSemaphore(LoadingNewLock);

        if (Loaded.TryGetValue(obj, out var v))
            return v;

        v = CreateNew(obj);

        using var @lock2 = new DeferredSemaphore(AccessReadCacheLock);
        if (!Loaded.TryAdd(obj, v))
            throw new global::System.Exception("Failed to add object to cache");
        return v;
    }

    /// <summary>
    /// Determines what must be done when an entirely new <typeparamref name="LoadedType"/> is created
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    protected abstract LoadedType CreateNew(UnloadedType raw);

    /// <summary>
    /// Dispose of any resources attached to the loaded type. This is invoked when an entry is unloaded.
    /// </summary>
    /// <param name="loaded"></param>
    protected abstract void DisposeOf(LoadedType loaded);

    /// <summary>
    /// Unload an entry and dispose of all attached resources
    /// </summary>
    public void Unload(UnloadedType obj)
    {
        using var @lock = new DeferredSemaphore(AccessReadCacheLock);

        if (!Loaded.Remove(obj, out var loadedObj))
        {
            Logger.Error($"Attempt to unload a(n) {typeof(UnloadedType).Name} that isn't loaded");
            return;
        }

        DisposeOf(loadedObj);
    }

    /// <summary>
    /// Returns if an entry is in the cache
    /// </summary>
    public bool Has(UnloadedType obj) => Loaded.ContainsKey(obj);

    /// <summary>
    /// Clear the cache
    /// </summary>
    public void UnloadAll()
    {
        using var @lock1 = new DeferredSemaphore(LoadingNewLock);
        using var @lock2 = new DeferredSemaphore(AccessReadCacheLock);

        foreach (var entry in Loaded)
            DisposeOf(entry.Value);
        Loaded.Clear();
    }

    /// <summary>
    /// Returns every loaded item in the cache
    /// </summary>
    public IEnumerable<LoadedType> GetAllLoaded()
    {
        using var @lock = new DeferredSemaphore(AccessReadCacheLock);

        foreach (var item in Loaded)
            yield return item.Value;
    }

    /// <summary>
    /// Returns every unloaded item ever loaded
    /// </summary>
    public IEnumerable<UnloadedType> GetAllUnloaded()
    {
        using var @lock = new DeferredSemaphore(AccessReadCacheLock);

        foreach (var item in Loaded)
            yield return item.Key;
    }

    /// <summary>
    /// Returns every unloaded item ever loaded
    /// </summary>
    public IEnumerable<(UnloadedType, LoadedType)> GetAll()
    {
        using var @lock = new DeferredSemaphore(AccessReadCacheLock);

        foreach (var item in Loaded)
            yield return (item.Key, item.Value);
    }
}

/// <summary>
/// A generic cache object that provides a way to load heavy objects based on a lighter key
/// </summary>
/// <typeparam name="UnloadedType">The key. This object is usually light and cheap to create</typeparam>
/// <typeparam name="LoadedType">The loaded object. This object is usually heavy and expensive to create</typeparam>
public abstract class Cache<UnloadedType, LoadedType> where UnloadedType : notnull
{
    protected readonly Dictionary<UnloadedType, LoadedType> Loaded = [];

    /// <summary>
    /// Load or create a <typeparamref name="LoadedType"/> from an <typeparamref name="UnloadedType"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual LoadedType Load(UnloadedType obj)
    {
        if (Loaded.TryGetValue(obj, out var v))
            return v;

        v = CreateNew(obj);
        Loaded.Add(obj, v);
        return v;
    }

    /// <summary>
    /// Determines what must be done when an entirely new <typeparamref name="LoadedType"/> is created
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    protected abstract LoadedType CreateNew(UnloadedType raw);

    /// <summary>
    /// Dispose of any resources attached to the loaded type. This is invoked when an entry is unloaded.
    /// </summary>
    /// <param name="loaded"></param>
    protected abstract void DisposeOf(LoadedType loaded);

    /// <summary>
    /// Unload an entry and dispose of all attached resources
    /// </summary>
    public void Unload(UnloadedType obj)
    {
        if (!Loaded.Remove(obj, out var loadedObj))
        {
            Logger.Error($"Attempt to unload a(n) {typeof(UnloadedType).Name} that isn't loaded");
            return;
        }

        DisposeOf(loadedObj);
    }

    /// <summary>
    /// Returns if an entry is in the cache
    /// </summary>
    public bool Has(UnloadedType obj) => Loaded.ContainsKey(obj);

    /// <summary>
    /// Clear the cache
    /// </summary>
    public void UnloadAll()
    {
        foreach (var entry in Loaded)
            DisposeOf(entry.Value);
        Loaded.Clear();
    }

    /// <summary>
    /// Returns every loaded item in the cache
    /// </summary>
    public IEnumerable<LoadedType> GetAllLoaded()
    {
        foreach (var item in Loaded)
            yield return item.Value;
    }

    /// <summary>
    /// Returns every unloaded item ever loaded
    /// </summary>
    public IEnumerable<UnloadedType> GetAllUnloaded()
    {
        foreach (var item in Loaded)
            yield return item.Key;
    }

    /// <summary>
    /// Returns every unloaded item ever loaded
    /// </summary>
    public IEnumerable<(UnloadedType, LoadedType)> GetAll()
    {
        foreach (var item in Loaded)
            yield return (item.Key, item.Value);
    }
}
