using System.Collections.Generic;

namespace Walgelijk;

/// <summary>
/// Abstract class for object pooling
/// </summary>
/// <typeparam name="T">The type to pool</typeparam>
/// <typeparam name="InitialData">The type of object to pass around when initalising a new instance</typeparam>
public abstract class Pool<T, InitialData> where T : class
{
    /// <summary>
    /// The maximum amount of objects, active or inactive
    /// </summary>
    public readonly int MaximumCapacity;

    /// <summary>
    /// How many objects have been created
    /// </summary>
    public int CreatedAmount { get; private set; } = 0;
    
    private readonly Stack<T> freeToUse;
    private readonly List<T> currentlyInUse;

    /// <summary>
    /// Enumerates through all objects currently in use
    /// </summary>
    public IEnumerable<T> GetAllInUse()
    {
        foreach (var item in currentlyInUse)
            yield return item;
    }

    /// <summary>
    /// Create a pool with the given capacity
    /// </summary>
    public Pool(int maxCapacity = 1000)
    {
        MaximumCapacity = maxCapacity;

        freeToUse = new(maxCapacity);
        currentlyInUse = new(maxCapacity);
    }

    /// <summary>
    /// Get an object from the pool. This will be ready for use or null, if the pool is at full capacity. 
    /// </summary>
    public virtual T? RequestObject(InitialData initialiser)
    {
        bool somethingAvailable = freeToUse.Count > 0;

        if (somethingAvailable)
            return GetExistingFromPool(initialiser);
        else if (CreatedAmount < MaximumCapacity)
        {
            Prefill();
            return GetExistingFromPool(initialiser);
        }
        else //over capacity
        {
            Logger.Warn("Pool can't satisfy demand because the MaximumCapacity is too low.", GetType().Name);
            return GetOverCapacityFallback();
        }
    }

    /// <summary>
    /// Return an object to the pool when it's done, to allow it to be requested again
    /// </summary>
    public virtual void ReturnToPool(T obj)
    {
        if (currentlyInUse.Remove(obj))
            freeToUse.Push(obj);
        else
            Logger.Warn("Object that was returned to the pool was already in the pool.", GetType().Name);
    }

    /// <summary>
    /// Create a new available object and add it to the pool, waiting to be used
    /// </summary>
    public virtual void Prefill()
    {
        var a = CreateFresh();
        CreatedAmount++;
        freeToUse.Push(a);
    }

    /// <summary>
    /// Get an existing object from the pool guaranteeing never to create a new one. Can return null.
    /// </summary>
    protected virtual T? GetExistingFromPool(InitialData initialiser)
    {
        if (!freeToUse.TryPop(out var f))
            return null;

        ResetObjectForNextUse(f, initialiser);
        currentlyInUse.Add(f);
        return f;
    }

    /// <summary>
    /// Return a completely new instance of the poolable object
    /// </summary>
    protected abstract T CreateFresh();

    /// <summary>
    /// Reset the given object for its next use
    /// </summary>
    protected abstract void ResetObjectForNextUse(T obj, InitialData initialiser);

    /// <summary>
    /// What to return instead of null when the pool is at full capacity
    /// </summary>
    protected abstract T GetOverCapacityFallback();
}


/// <summary>
/// Abstract class for object pooling
/// </summary>
/// <typeparam name="T">The type to pool</typeparam>
public abstract class Pool<T> where T : class
{
    /// <summary>
    /// The maximum amount of objects, active or inactive
    /// </summary>
    public readonly int MaximumCapacity;

    /// <summary>
    /// How many objects have been created
    /// </summary>
    public int CreatedAmount { get; private set; } = 0;

    private readonly Stack<T> freeToUse;
    private readonly List<T> currentlyInUse;

    /// <summary>
    /// Enumerates through all objects currently in use
    /// </summary>
    public IEnumerable<T> GetAllInUse()
    {
        foreach (var item in currentlyInUse)
            yield return item;
    }

    /// <summary>
    /// Create a pool with the given capacity
    /// </summary>
    public Pool(int maxCapacity = 1000)
    {
        MaximumCapacity = maxCapacity;

        freeToUse = new(maxCapacity);
        currentlyInUse = new(maxCapacity);
    }

    /// <summary>
    /// Get an object from the pool. This will be ready for use or null, if the pool is at full capacity. 
    /// </summary>
    public virtual T? RequestObject()
    {
        bool somethingAvailable = freeToUse.Count > 0;

        if (somethingAvailable)
            return GetExistingFromPool();
        else if (CreatedAmount < MaximumCapacity)
        {
            Prefill();
            return GetExistingFromPool();
        }
        else //over capacity
        {
            Logger.Warn("Pool can't satisfy demand because the MaximumCapacity is too low.", GetType().Name);
            return GetOverCapacityFallback();
        }
    }

    /// <summary>
    /// Return an object to the pool when it's done, to allow it to be requested again
    /// </summary>
    public virtual void ReturnToPool(T obj)
    {
        if (currentlyInUse.Remove(obj))
            freeToUse.Push(obj);
        else
            Logger.Warn("Object that was returned to the pool was already in the pool.", GetType().Name);
    }

    /// <summary>
    /// Create a new available object and add it to the pool, waiting to be used
    /// </summary>
    public virtual void Prefill()
    {
        var a = CreateFresh();
        CreatedAmount++;
        freeToUse.Push(a);
    }

    /// <summary>
    /// Get an existing object from the pool guaranteeing never to create a new one. Can return null.
    /// </summary>
    protected virtual T? GetExistingFromPool()
    {
        if (!freeToUse.TryPop(out var f))
            return null;

        ResetObjectForNextUse(f);
        currentlyInUse.Add(f);
        return f;
    }

    /// <summary>
    /// Return a completely new instance of the poolable object
    /// </summary>
    protected abstract T CreateFresh();

    /// <summary>
    /// Reset the given object for its next use
    /// </summary>
    protected abstract void ResetObjectForNextUse(T obj);

    /// <summary>
    /// What to return instead of null when the pool is at full capacity
    /// </summary>
    protected abstract T GetOverCapacityFallback();
}
