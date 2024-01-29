using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK;

public class VertexBufferCacheCache
{
    private readonly Dictionary<Type, object> dictionary = [];

    public VertexBufferCache<TVertex> Load<TVertex>() where TVertex : struct
    {
        var key = typeof(TVertex);

        if (dictionary.TryGetValue(key, out var potentialCache)) // return if found
            return potentialCache as VertexBufferCache<TVertex>;

        // create new 
        var v = new VertexBufferCache<TVertex>();
        dictionary.Add(key, v);
        return v;
    }
}