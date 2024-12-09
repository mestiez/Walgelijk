using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Walgelijk;

[Experimental("TEXTUREATLAS")]
public class TextureAtlas
{
    public IReadableTexture Page => page ?? (IReadableTexture)Texture.ErrorTexture;

    public void Add(IReadableTexture texture, in string name) => Add(texture, name.AsSpan());
    public Vector4 GetUvRect(in string name) => GetUvRect(name.AsSpan());
    public static int StringToId(in ReadOnlySpan<char> id) => Hashes.MurmurHash1(id);
    public static int StringToId(in string id) => Hashes.MurmurHash1(id);

    private RenderTexture? page;
    private readonly Dictionary<int, Entry> packed = [];
    private readonly SemaphoreSlim atlasLock = new(1);

    public void Add(IReadableTexture texture, in ReadOnlySpan<char> name)
    {
        using var l = new DeferredSemaphore(atlasLock);

        var id = StringToId(name);
        var entry = new Entry
        {
            Id = id,
            Texture = texture
        };
        packed.AddOrSet(id, entry);
    }

    public Vector4 GetUvRect(in ReadOnlySpan<char> name)
    {
        using var l = new DeferredSemaphore(atlasLock);

        var id = StringToId(name);
        if (packed.TryGetValue(id, out var r))
            return r.UvRect;

        return new Vector4(0, 0, 1, 1);
    }

    /// <summary>
    /// Add the build task to the RenderQueue
    /// </summary>
    /// <param name="renderQueue"></param>
    public void Build(RenderQueue renderQueue)
    {
        renderQueue.Add(new ActionRenderTask(Build));
    }

    /// <summary>
    /// Render task function that renders the page texture
    /// </summary>
    /// <param name="graphics"></param>
    public void Build(IGraphics graphics)
    {
        using var l = new DeferredSemaphore(atlasLock);

        Entry[] sorted = [.. packed.Values.OrderBy(static e => e.Texture.Width * e.Texture.Width)];
        var cursor = new Vector2();
        foreach (var entry in sorted)
        {

        }

        RenderAtlas(sorted, graphics);
    }

    private void RenderAtlas(Entry[] set, IGraphics g)
    {
        if (set.Length == 0)
        {
            page = new RenderTexture(32, 32, filterMode: FilterMode.Nearest, flags: RenderTargetFlags.None);
            g.ActOnTarget(page, g => { g.Clear(Colors.Magenta); });
            return;
        }

        var w = int.Max(32, (int)BitOperations.RoundUpToPowerOf2((uint)float.Ceiling(set.Max(e => e.PackedRect.MaxX))));
        var h = int.Max(32, (int)BitOperations.RoundUpToPowerOf2((uint)float.Ceiling(set.Max(e => e.PackedRect.MaxY))));

        float scalingX = 1f / w;
        float scalingY = 1f / h;

        page?.Dispose();
        page = new RenderTexture(w, h, filterMode: FilterMode.Nearest, flags: RenderTargetFlags.None);
        page.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, w, h, 0, 0, 100);

        g.ActOnTarget(page, g =>
        {
            using var tempMat = new Material();
            g.Clear(Colors.Magenta);
            foreach (var item in set)
            {
                var rect = item.PackedRect;
                tempMat.SetUniform("mainTex", item.Texture);
                g.DrawQuadScreenspace(rect, tempMat);
                item.UvRect = new Vector4(
                    scalingX * rect.MinX,
                    scalingX * rect.MinY,
                    scalingY * rect.MaxX,
                    scalingY * rect.MaxY);
            }
            tempMat.Dispose();
        });
    }

    private class Entry
    {
        public required int Id { init; get; }
        public required IReadableTexture Texture { init; get; }

        public Rect PackedRect;
        public Vector4 UvRect;
    }
}
