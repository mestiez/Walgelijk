using System;
using System.Collections.Generic;

namespace Walgelijk;

/// <summary>
/// Event that can be dispatched and listened to
/// </summary>
public class Hook<T>
{
    private readonly List<Action<T>> listeners = new ();

    /// <summary>
    /// Dispatch the event to all listeners
    /// </summary>
    public void Dispatch(T args)
    {
        foreach (var listener in listeners)
            listener?.Invoke(args);
    }

    /// <summary>
    /// Adds the given action to the listeners list
    /// </summary>
    public void AddListener(Action<T> action)
    {
        listeners.Add(action);
    }

    /// <summary>
    /// Remove the given action from the listeners list
    /// </summary>
    public void RemoveListener(Action<T> action)
    {
        listeners.Remove(action);
    }

    /// <summary>
    /// Returns if the given action will be invoked when the event is dispatched
    /// </summary>
    public bool HasListener(Action<T> action)
    {
        return listeners.Contains(action);
    }

    /// <summary>
    /// Clear all listeners
    /// </summary>
    public void ClearListeners()
    {
        listeners.Clear();
    }

    /// <summary>
    /// Amount of listeners
    /// </summary>
    public int ListenerCount => listeners.Count;
}

/// <summary>
/// Event that can be dispatched and listened to
/// </summary>
public class Hook
{
    private readonly List<Action> listeners = new();

    /// <summary>
    /// Dispatch the event to all listeners
    /// </summary>
    public void Dispatch()
    {
        foreach (var listener in listeners)
            listener?.Invoke();
    }

    /// <summary>
    /// Adds the given action to the listeners list
    /// </summary>
    public void AddListener(Action action)
    {
        listeners.Add(action);
    }

    /// <summary>
    /// Remove the given action from the listeners list
    /// </summary>
    public void RemoveListener(Action action)
    {
        listeners.Remove(action);
    }

    /// <summary>
    /// Returns if the given action will be invoked when the event is dispatched
    /// </summary>
    public bool HasListener(Action action)
    {
        return listeners.Contains(action);
    }

    /// <summary>
    /// Clear all listeners
    /// </summary>
    public void ClearListeners()
    {
        listeners.Clear();
    }

    /// <summary>
    /// Amount of listeners
    /// </summary>
    public int ListenerCount => listeners.Count;
}
