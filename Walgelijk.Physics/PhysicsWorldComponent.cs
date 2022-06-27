using System;
using System.Collections.Generic;

namespace Walgelijk.Physics
{
    /// <summary>
    /// Physics world component. Contains all data that describes a physics world
    /// </summary>
    public class PhysicsWorldComponent
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
        public Dictionary<IntVector2, Chunk> ChunkDictionary = new();

        /// <summary>
        /// Collision grid chunk size
        /// </summary>
        public float ChunkSize = 3;

        /// <summary>
        /// Max amount of bodies per chunk
        /// </summary>
        public int ChunkCapacity = 64;
    }

    public struct IntVector2
    {
        public int X, Y;

        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is IntVector2 vector &&
                   X == vector.X &&
                   Y == vector.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(IntVector2 left, IntVector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntVector2 left, IntVector2 right)
        {
            return !(left == right);
        }
    }

    public class Chunk
    {
        public int X, Y;
        public Entity[] ContainingEntities;
        public int BodyCount;

        public Chunk(int x, int y, int capacity = 64)
        {
            X = x;
            Y = y;
            ContainingEntities = new Entity[capacity];
        }

        public void Add(Entity e)
        {
            if (BodyCount >= ContainingEntities.Length)
            {
                Logger.Warn("Chunk overflow!");
                return;
            }

            ContainingEntities[BodyCount] = e;
            BodyCount++;
        }

        public void Clear()
        {
            BodyCount = 0;
        }

        public bool IsEmpty => BodyCount <= 0;
    }
}
