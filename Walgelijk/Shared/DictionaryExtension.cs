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
}