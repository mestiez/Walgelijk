using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Walgelijk;

/// <summary>
/// An array with a fixed capacity that keeps track of how many objects were entered;
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ClampedArray<T> : ICloneable, IList<T>
{
    public readonly int Capacity;

    private readonly T[] array;
    private int count;

    public ClampedArray(int capacity)
    {
        array = new T[capacity];

        Capacity = array.Length;
    }

    public ClampedArray(params T[] vals)
    {
        array = new T[vals.Length];
        Array.Copy(vals, array, vals.Length);

        Capacity = array.Length;
    }

    public ClampedArray(int length, params T[] vals)
    {
        var filled = Math.Min(length, vals.Length);
        array = new T[length];
        Array.Copy(vals, array, filled);
        count = filled;
        Capacity = array.Length;
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException("Attempt to access index out of range");
            return array[index];
        }
        set
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException("Attempt to access index out of range");
            array[index] = value;
        }
    }

    public int Count => count;

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        if (count >= Capacity)
            throw new IndexOutOfRangeException("Attempt to add item to array that is at capacity");
        array[count] = item;
        count++;
    }

    public void Clear()
    {
        count = 0;
    }

    public void Clear(bool zero)
    {
        count = 0;
        Array.Clear(array);
    }

    public object Clone() => new ClampedArray<T>(Capacity, array);

    public bool Contains(T item)
    {
        for (int i = 0; i < count; i++)
            if (array[i]?.Equals(item) ?? false)
                return true;
        return false;
    }

    public void CopyTo(T[] other, int arrayIndex)
    {
        int mi = 0;
        int oi = arrayIndex;
        while (mi < count && oi < other.Length)
        {
            other[oi] = array[mi];
            mi++;
            oi++;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < count; i++)
            yield return array[i];
    }

    public int IndexOf(T item)
    {
        for (int i = 0; i < count; i++)
            if (array[i]?.Equals(item) ?? false)
                return i;
        return -1;
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Insert(int index, T item) => throw new NotImplementedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public bool Remove(T item) => throw new NotImplementedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void RemoveAt(int index) => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < count; i++)
            yield return array[i];
    }

    /// <summary>
    /// Creates a new span over the target array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return new Span<T>(array, 0, Count);
    }

    /// <summary>
    /// Creates a new Span over the portion of the target array beginning
    /// at 'start' index and ending at 'end' index (exclusive).
    /// </summary>
    /// <param name="array">The target array.</param>
    /// <param name="start">The index at which to begin the Span.</param>
    /// <param name="length">The number of items in the Span.</param>
    /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
    /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start, int length)
    {
        return array.AsSpan(0, Count).Slice(start, length);
    }
}