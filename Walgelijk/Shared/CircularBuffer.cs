using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Basic circular buffer implementation
/// </summary>
/// <typeparam name="T"></typeparam>
public class CircularBuffer<T> where T : IEqualityOperators<T, T, bool>
{
    private readonly T[] buffer;
    private int index = 0;

    /// <summary>
    /// Total capacity
    /// </summary>
    public int Capacity => buffer.Length;

    /// <summary>
    /// Current position in the buffer
    /// </summary>
    public int Position => index;

    public CircularBuffer(int capacity)
    {
        buffer = new T[capacity];
    }

    /// <summary>
    /// Add <paramref name="value"/> to the buffer at <see cref="Position"/>
    /// </summary>
    /// <param name="value"></param>
    public void Write(T value)
    {
        buffer[index] = value;
        index++;
        index %= buffer.Length;
    }  
    
    /// <summary>
    /// Add <paramref name="value"/> to the buffer at <see cref="Position"/>
    /// </summary>
    /// <param name="value"></param>
    public void Write(ReadOnlySpan<T> values)
    {
        for (int i = 0; i < values.Length; i++)
            Write(values[i]);
    }

    /// <summary>
    /// Take value from the buffer at <see cref="Position"/>
    /// </summary>
    /// <returns></returns>
    public T Read()
    {
        var v = buffer[index];

        index--;
        if (index < 0)
            index = Capacity - 1;

        return v;
    }

    public bool Contains(T value)
    {
        // TODO maybe optimise with a binary search tree thing
        for (int i = 0; i < buffer.Length; i++)
            if (buffer[i] == value)
                return true;
        return false;
    }
}
