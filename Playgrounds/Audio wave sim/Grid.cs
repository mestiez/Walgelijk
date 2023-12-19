//#define HILBERT

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Playgrounds;

public class Grid<T> : IEnumerable<(int X, int Y, T Value)> where T : notnull
{
    public readonly int Width, Height;
    public T[] Flat;
    private int[] indexByPos;

    public Grid(int width, int height, T initialValue)
    {
        Width = width;
        Height = height;
        Flat = new T[width * height];
        Array.Fill(Flat, initialValue);
        InitGrid(width, height);
    }


    public Grid(int width, int height, T[] flat)
    {
        if (flat.Length != width * height)
            throw new ArgumentException("Given array is not of size width * height");
        Width = width;
        Height = height;
        Flat = flat;
        InitGrid(width, height);
    }

    private void InitGrid(int width, int height)
    {
        indexByPos = new int[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                indexByPos[y * width + x] = GetIndexFrom(x, y);
            }
        }
    }

    public void Fill(T value)
    {
        Array.Fill(Flat, value);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public T Get(int x, int y)
    {
        return Flat[GetIndexFromCache(x, y)];
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private int GetIndexFromCache(int x, int y)
    {
        return indexByPos[y * Width + x];
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryGet(int x, int y, [NotNullWhen(true)] out T? cell)
    {
        cell = default;
        if (x < 0 || x >= Width) return false;
        if (y < 0 || y >= Height) return false;

        cell = Flat[GetIndexFromCache(x, y)];
        return true;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y, T value)
    {
        Flat[GetIndexFromCache(x, y)] = value;
    }

    public IEnumerator<(int X, int Y, T Value)> GetEnumerator()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                yield return (x, y, Get(x, y));
    }

    public bool BlockEquals(Grid<T> block, int top = 0, int left = 0)
    {
        foreach (var (X, Y, Value) in block)
        {
            int tx = X + left;
            int ty = Y + top;
            if (tx < 0 || tx >= Width)
                return false;
            if (ty < 0 || ty >= Height)
                return false;
            if (!Get(tx, ty).Equals(Value))
                return false;
        }
        return true;
    }

    public void WriteBlock(Grid<T> block, int top, int left)
    {
        foreach (var (X, Y, Value) in block)
        {
            int tx = X + left;
            int ty = Y + top;
            Set(tx, ty, Value);
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetIndexFrom(int x, int y)
    {
        if (x < 0 || x >= Width)
            throw new ArgumentOutOfRangeException($"X ({x}) is out of range (0, {Width - 1})");

        if (y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException($"Y ({y}) is out of range (0, {Height - 1})");

        return IndexFromCoordinates(x, y, Width, Height);
    }

    public static T GetAt(T[] flat, int x, int y, int w, int h) => flat[IndexFromCoordinates(x, y, w, h)];
    public static void SetAt(T[] flat, int x, int y, int w, int h, T val) => flat[IndexFromCoordinates(x, y, w, h)] = val;
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void GetCoordinatesFromIndex(int index, out int x, out int y)
    {
        if (index >= Width * Height)
            throw new ArgumentOutOfRangeException($"Index ({index}) is out of range ({Width * Height})");

        GetCoordinatesFromIndex(index, Width, Height, out x, out y);
    }

    //The actual index functions. Mapping a single integer to a position on the grid
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int IndexFromCoordinates(int x, int y, int width, int height)
    {
#if HILBERT
        int n = height;
        int rx, ry, s, d = 0;
        for (s = n / 2; s > 0; s /= 2)
        {
            rx = (x & s) > 0 ? 1 : 0;
            ry = (y & s) > 0 ? 1 : 0;
            d += s * s * ((3 * rx) ^ ry);
            HilbertRotate(n, ref x, ref y, rx, ry);
        }
        return d;
#else
        return (width * y + x);
#endif
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void GetCoordinatesFromIndex(int d, int width, int height, out int x, out int y)
    {
#if HILBERT
        var n = height;

        int rx, ry, s, t = d;
        x = y = 0;
        for (s = 1; s < n; s *= 2)
        {
            rx = 1 & (t / 2);
            ry = 1 & (t ^ rx);
            HilbertRotate(s, ref x, ref y, rx, ry);
            x += s * rx;
            y += s * ry;
            t /= 4;
        }
#else
        x = d % width;
        y = d / width;
#endif
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void HilbertRotate(int n, ref int x, ref int y, int rx, int ry)
    {
        if (ry == 0)
        {
            if (rx == 1)
            {
                x = n - 1 - x;
                y = n - 1 - y;
            }

            //Swap x and y
            int t = x;
            x = y;
            y = t;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}