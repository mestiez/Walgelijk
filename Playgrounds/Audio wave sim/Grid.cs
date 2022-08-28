using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace TestWorld;

public class Grid<T> : IEnumerable<(int X, int Y, T Value)> where T : notnull
{
    public readonly int Width, Height;
    public T[] Flat;

    public Grid(int width, int height, T initialValue)
    {
        Width = width;
        Height = height;
        Flat = new T[width * height];
        Array.Fill(Flat, initialValue);
    }

    public Grid(int width, int height, T[] flat)
    {
        if (flat.Length != width * height)
            throw new ArgumentException("Given array is not of size width * height");
        Width = width;
        Height = height;
        Flat = flat;
    }

    public void Fill(T value)
    {
        Array.Fill(Flat, value);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public T Get(int x, int y)
    {
        return Flat[GetIndexFrom(x, y)];
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryGet(int x, int y, [NotNullWhen(true)] out T? cell)
    {
        cell = default;
        if (x < 0 || x >= Width) return false;
        if (y < 0 || y >= Height) return false;

        cell = Flat[GetIndexFrom(x, y)];
        return true;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y, T value)
    {
        Flat[GetIndexFrom(x, y)] = value;
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

        return IndexFromCoordinates(x, y, Width);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int IndexFromCoordinates(int x, int y, int w) => w * y + x;
    public static T GetAt(T[] flat, int x, int y, int w) => flat[IndexFromCoordinates(x, y, w)];
    public static void SetAt(T[] flat, int x, int y, int w, T val) => flat[IndexFromCoordinates(x, y, w)] = val;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void GetCoordinatesFromIndex(int index, out int x, out int y)
    {
        if (index >= Width * Height)
            throw new ArgumentOutOfRangeException($"Index ({index}) is out of range ({Width * Height})");

        GetCoordinatesFromIndex(index, Width, out x, out y);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void GetCoordinatesFromIndex(int index, int w, out int x, out int y)
    {
        x = index % w;
        y = index / w;
    }


    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}