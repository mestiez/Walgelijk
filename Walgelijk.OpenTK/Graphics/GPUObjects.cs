using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK;

internal static class GPUObjects
{
    internal static readonly MaterialCache MaterialCache = new();
    internal static readonly TextureCache TextureCache = new();
    internal static readonly ShaderCache ShaderCache = new();
    internal static readonly MaterialTextureCache MaterialTextureCache = new();

    internal static readonly VertexBufferCacheCache VertexBufferCache = new();
    internal static readonly RenderTextureCache RenderTextureCache = new();

    internal static readonly RenderTargetDictionary RenderTargetDictionary = new();
}

public class VertexBufferCacheCache
{
    private readonly Dictionary<(Type, Type), object> dictionary = [];

    public VertexBufferCache<TVertex, TDescriptor> Load<TVertex, TDescriptor>() where TDescriptor : IVertexDescriptor<TVertex>, new() where TVertex : struct
    {
        var key = (typeof(TVertex), typeof(TDescriptor));

        if (dictionary.TryGetValue(key, out var potentialCache)) // return if found
            return potentialCache as VertexBufferCache<TVertex, TDescriptor>;

        // create new 
        var v = new VertexBufferCache<TVertex, TDescriptor>();
        dictionary.Add(key, v);
        return v;
    }
}