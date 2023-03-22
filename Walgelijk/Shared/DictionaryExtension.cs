using System.Collections.Generic;

namespace Walgelijk;

public static class DictionaryExtension
{
    public static TValue Ensure<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        if (!dict.TryGetValue(key, out var value))
        {
            value = new();
            dict.Add(key, value);
        }
        return value;
    }

    public static void AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue set)
    {
        if (!dict.TryAdd(key, set))
            dict[key] = set;
    }

    public static TValue Ensure<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, out bool isNew) where TValue : new()
    {
        isNew = true;
        if (!dict.TryGetValue(key, out var value))
        {
            value = new();
            dict.Add(key, value);
            isNew = false;
        }
        return value;
    }
}