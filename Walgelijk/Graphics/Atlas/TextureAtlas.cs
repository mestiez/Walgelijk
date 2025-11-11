using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using static Walgelijk.IBinPacker;

namespace Walgelijk;

/// <summary>
/// Represents a texture atlas that packs multiple textures into a single page texture and provides UV mapping for each
/// entry.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify individual textures within the atlas. Must be non-nullable.</typeparam>
public class TextureAtlas<TKey> where TKey : notnull
{
    public IReadableTexture Page => page ?? (IReadableTexture)Texture.ErrorTexture;
    public int Padding = 0;
    public required IBinPacker BinPacker;
    public int MaxWidth = 256, MaxHeight = int.MaxValue;

    public IEnumerable<TKey> Entries => packed.Keys;

    private RenderTexture? page;
    private readonly Dictionary<TKey, Entry> packed = [];
    private readonly SemaphoreSlim atlasLock = new(1);

    /// <summary>
    /// Adds a texture to the atlas and associates it with the specified key. If an entry with the same key already
    /// exists, it is replaced.
    /// </summary>
    /// <param name="texture">The texture to add to the atlas. Cannot be null.</param>
    /// <param name="key">The key to associate with the texture. If the key already exists, its associated texture is updated.</param>
    public void Add(IReadableTexture texture, TKey key)
    {
        using var l = new DeferredSemaphore(atlasLock);

        var entry = new Entry
        {
            Id = key,
            Texture = texture
        };
        packed.AddOrSet(key, entry);
    }

    /// <summary>
    /// Removes all elements from the collection.
    /// </summary>
    public void Clear()
    {
        packed.Clear();
    }

    /// <summary>
    /// Gets the UV rectangle associated with the specified key.
    /// </summary>
    /// <param name="key">The key for which to retrieve the UV rectangle.</param>
    /// <returns>A <see cref="Vector4"/> representing the UV rectangle for the specified key. Returns a rectangle covering the full
    /// texture (0, 0, 1, 1) if the key is not found.</returns>
    public Vector4 GetUvRect(in TKey key)
    {
        using var l = new DeferredSemaphore(atlasLock);

        if (packed.TryGetValue(key, out var r))
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
        Entry[] sorted = [.. packed.Values.OrderByDescending(static e => int.Max(e.Texture.Height, e.Texture.Width))];
        IBin[] bins = [.. sorted.Cast<IBin>()];

        foreach (var item in sorted)
            item.PackedRect = new Rect(0, 0, item.Texture.Width, item.Texture.Height);

        int incr = 1;
        while (true)
        {
            var container = new Rect(0, 0, MaxWidth * incr, MaxHeight * incr);
            if (BinPacker.Pack(bins, container, Padding) || incr > 4)
                break;

            incr++;
        }

        RenderAtlas(sorted, graphics);
    }

    private void RenderAtlas(Entry[] set, IGraphics g)
    {
        if (set.Length == 0)
        {
            page = new RenderTexture(32, 32, filterMode: FilterMode.Nearest, flags: RenderTargetFlags.None);
            g.ActOnTarget(page, g => { g.Clear(Colors.Transparent); });
            return;
        }

        var w = int.Max(32, (int)BitOperations.RoundUpToPowerOf2((uint)float.Ceiling(Padding + set.Max(e => e.PackedRect.MaxX))));
        var h = int.Max(32, (int)BitOperations.RoundUpToPowerOf2((uint)float.Ceiling(Padding + set.Max(e => e.PackedRect.MaxY))));

        float scalingX = 1f / w;
        float scalingY = 1f / h;

        page?.Dispose();
        page = new RenderTexture(w, h, filterMode: FilterMode.Nearest, flags: RenderTargetFlags.None);
        page.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, w, 0, h, 0, 100);

        g.ActOnTarget(page, g =>
        {
            using var tempMat = new Material();
            g.Clear(Colors.Transparent);
            foreach (var item in set)
            {
                var rect = item.PackedRect;
                rect.MinX = float.Floor(rect.MinX);
                rect.MinY = float.Floor(rect.MinY);
                rect.MaxX = rect.MinX + item.Texture.Width;
                rect.MaxY = rect.MinY + item.Texture.Height;

                tempMat.SetUniform("mainTex", item.Texture);
                g.DrawQuadScreenspace(rect, tempMat);
                item.UvRect = new Vector4(
                    scalingX * rect.MinX,
                    scalingY * rect.MinY,
                    scalingX * rect.MaxX,
                    scalingY * rect.MaxY);
            }
            tempMat.Dispose();
        });
    }

    private class Entry : IBin
    {
        public required TKey Id { init; get; }
        public required IReadableTexture Texture { init; get; }

        Vector2 IBin.Min { get => new Vector2(PackedRect.MinX, PackedRect.MinY); set { PackedRect.MinX = value.X; PackedRect.MinY = value.Y; } }
        Vector2 IBin.Max { get => new Vector2(PackedRect.MaxX, PackedRect.MaxY); set { PackedRect.MaxX = value.X; PackedRect.MaxY = value.Y; } }

        public Rect PackedRect;
        public Vector4 UvRect;
    }
}

/// <summary>
/// Interface for packing rectangles into a container while considering existing rectangles.
/// </summary>
public interface IBinPacker
{
    /// <summary>
    /// Pack a rectangle into the given container taking all rectangles in <see cref="OtherRects"/> into account
    /// </summary>
    public bool Pack(Span<IBin> bins, in Rect container, int padding = 0);

    public interface IBin
    {
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }

        public float Width => Max.X - Min.X;
        public float Height => Max.Y - Min.Y;

        public float Area => Width * Height;

        public void SetFromRect(Rect r)
        {
            Min = new Vector2(r.MinX, r.MinY);
            Max = new Vector2(r.MaxX, r.MaxY);
        }

        public Rect AsRect => new Rect(Min.X, Min.Y, Max.X, Max.Y);
    }
}

/// <summary>
/// Shelf bin packer with gravity
/// Incredibly slow
/// </summary>
public class GravityBinPacker : IBinPacker
{
    public bool Pack(Span<IBin> bins, in Rect container, int padding = 0)
    {
        if (!new ShelfBinPacker().Pack(bins, container, padding))
            return false;

        while (true)
        {
            int changes = 0;

            foreach (var bin in bins)
            {
                if (moveIfPossible(bin, bins, -1, 0)) changes++;
                if (moveIfPossible(bin, bins, 0, -1)) changes++;
            }

            if (changes == 0)
                return true;
        }

        bool moveIfPossible(IBin bin, Span<IBin> bins, int x, int y)
        {
            x = int.Sign(x);
            y = int.Sign(y);

            var aRect = bin.AsRect.Translate(x, y).SortComponents();

            if (aRect.MinX < padding || aRect.MinY < padding)
                return false;

            foreach (var b in bins)
            {
                if (bin == b)
                    continue;

                var bRect = b.AsRect.Expand(padding);

                if (aRect.IntersectsRectangle(bRect.Expand(-1)))
                    return false;
            }

            bin.SetFromRect(aRect);
            return true;
        }
    }
}

/// <summary>
/// The most naive bin packer
/// </summary>
public class ShelfBinPacker : IBinPacker
{
    public bool Pack(Span<IBin> bins, in Rect container, int padding = 0)
    {
        var area = 0f;
        foreach (var item in bins)
            area += item.Width * item.Height;
        var side = float.Sqrt(area) * 2;

        var cursor = new Vector2(padding);
        float rowHeight = padding;
        foreach (var bin in bins)
        {
            if (cursor.X + bin.Width >= float.Min(side, container.Width))
            {
                cursor.X = padding;
                cursor.Y += rowHeight + padding;
                rowHeight = 0;
            }

            bin.SetFromRect(new Rect(0, 0, bin.Width, bin.Height).Translate(cursor.X, cursor.Y));

            cursor.X += bin.Width + padding;
            rowHeight = float.Max(rowHeight, bin.Height);
        }

        return true;
    }
}

/// <summary>
/// Absolutely terrible bin packer
/// </summary>
public class GuillotineBinPacker : IBinPacker
{
    public Fit Mode = Fit.BestAreaFit;
    public SplitDirection Direction;

    public enum Fit
    {
        BestAreaFit,
        BestShortSideFit,
        BestLongSideFit,
        BestAspectRatioFit,
        WorstAreaFit,
        FirstFit,
        WorstShortSideFit,
        WorstLongSideFit,
    }

    public enum SplitDirection
    {
        ShortSide,
        LongSide
    }

    public bool Pack(Span<IBin> bins, in Rect container, int padding = 0)
    {
        var area = 0f;
        foreach (var item in bins)
            area += item.Width * item.Height;
        var side = float.Sqrt(area) * 2;

        var freeSpace = new HashSet<Rect> { new Rect(0, 0, side, side) };

        foreach (var bin in bins)
        {
            Rect[] freeSpaceCopy = [.. freeSpace];

            if (TryGetBestFit(bin, freeSpaceCopy, out var space, padding))
            {
                var r = new Rect(0, 0, bin.Width + padding, bin.Height + padding).Translate(space.MinX + padding, space.MinY + padding).SortComponents();
                bin.SetFromRect(r);
                freeSpace.RemoveWhere(s =>
                    float.Abs(s.MinY - space.MinY) < 0.1f &&
                    float.Abs(s.MinX - space.MinX) < 0.1f &&
                    float.Abs(s.MaxX - space.MaxX) < 0.1f &&
                    float.Abs(s.MaxY - space.MaxY) < 0.1f
                );

                switch (Direction)
                {
                    case SplitDirection.ShortSide:
                        if (r.Width <= r.Height)
                        {
                            freeSpace.Add(space with { MinX = r.MaxX });
                            freeSpace.Add(space with { MinY = r.MaxY, MaxX = r.MaxX });
                        }
                        else
                        {
                            freeSpace.Add(space with { MinY = r.MaxY });
                            freeSpace.Add(space with { MinX = r.MaxX, MaxY = r.MaxY });
                        }
                        break;
                    case SplitDirection.LongSide:
                        if (r.Width > r.Height)
                        {
                            freeSpace.Add(space with { MinX = r.MaxX });
                            freeSpace.Add(space with { MinY = r.MaxY, MaxX = r.MaxX });
                        }
                        else
                        {
                            freeSpace.Add(space with { MinY = r.MaxY });
                            freeSpace.Add(space with { MinX = r.MaxX, MaxY = r.MaxY });
                        }
                        break;
                }
            }
            else return false;
        }

        return true;
    }

    private bool TryGetBestFit(IBin bin, Rect[] freeSpace, out Rect fit, int padding = 0)
    {
        var pad2 = padding * 2;
        var candidates = freeSpace.Where(r => r.Width >= bin.Width + pad2 && r.Height >= bin.Height + pad2).ToArray();
        if (candidates.Length == 0)
        {
            fit = default;
            return false;
        }

        var areaAdd = pad2 * pad2 + bin.Width * pad2 + bin.Height * pad2;
        var paddedBinArea = bin.Area + areaAdd;
        var paddedWidth = bin.Width + pad2;
        var paddedHeight = bin.Height + pad2;

        switch (Mode)
        {
            case Fit.FirstFit:
                fit = candidates.FirstOrDefault();
                return fit != default;
            case Fit.BestAreaFit:
                fit = candidates.MinBy(r => r.Area - paddedBinArea);
                return true;
            case Fit.BestShortSideFit:
                fit = candidates.MinBy(r => float.Min(r.Width - paddedWidth, r.Height - paddedHeight));
                return true;
            case Fit.BestLongSideFit:
                fit = candidates.MinBy(r => float.Max(r.Width - paddedWidth, r.Height - paddedHeight));
                return true;
            case Fit.BestAspectRatioFit:
                fit = candidates.MinBy(r => float.Abs((r.Width / r.Height) - (bin.Width / bin.Height)));
                return true;
            case Fit.WorstAreaFit:
                fit = candidates.MaxBy(r => r.Area - paddedBinArea);
                return true;
            case Fit.WorstShortSideFit:
                fit = freeSpace[0];
                return true;
            case Fit.WorstLongSideFit:
                fit = freeSpace[0];
                return true;
            default:
                fit = freeSpace[0];
                return true;
        }
    }
}