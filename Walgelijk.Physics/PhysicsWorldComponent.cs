using System;
using System.Collections.Generic;

namespace Walgelijk.Physics;

/// <summary>
/// Physics world component. Contains all data that describes a physics world
/// </summary>
[SingleInstance]
public class PhysicsWorldComponent : Component, IDisposable
{
    /// <summary>
    /// Physics updates per second
    /// </summary>
    public int UpdatesPerSecond = 60;

    /// <summary>
    /// All bodies are within these bounds
    /// </summary>
    public Rect MaxBodyEnvelopingBounds;

    /// <summary>
    /// Collision grid
    /// </summary>
    public readonly Dictionary<IntVector2, Chunk> ChunkDictionary = new();

    /// <summary>
    /// Collision grid chunk size
    /// </summary>
    public float ChunkSize = 3;

    /// <summary>
    /// Max amount of bodies per chunk
    /// </summary>
    public int ChunkCapacity = 64;

    public void Dispose()
    {
        foreach (var item in ChunkDictionary.Values)
        {
            item.Clear();
            Array.Clear(item.ContainingEntities);
        }

        ChunkDictionary.Clear();
    }
}
