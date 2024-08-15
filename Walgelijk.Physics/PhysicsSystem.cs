using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Walgelijk.Physics;

public class PhysicsSystem : Walgelijk.System
{
    //private readonly FixedIntervalDistributor distributor = new();

    public PhysicsSystem()
    {
        ExecutionOrder = -1;
    }

    public override void Update()
    {
        if (!Scene.FindAnyComponent<PhysicsWorldComponent>(out var world))
        {
            Logger.Warn("PhysicsSystem without PhysicsWorld...");
            return;
        }

        var bodies = Scene.GetAllComponentsOfType<PhysicsBodyComponent>();

        CalculateWorldBounds(world, bodies);
        CalculateCollisionChunks(world, bodies);
    }

    public static int GetChunkComponentFrom(float x, float chunkSize) => (int)float.Floor(x / chunkSize);

    public static IntVector2 GetChunkPositionFrom(float x, float y, float chunkSize) => new(GetChunkComponentFrom(x, chunkSize), GetChunkComponentFrom(y, chunkSize));

    public bool IsAnythingInChunk(float worldX, float worldY, PhysicsWorldComponent world) => IsAnythingInChunk(GetChunkPositionFrom(worldX, worldY, world.ChunkSize), world);

    public IEnumerable<Entity> GetBodiesNear(float worldX, float worldY, PhysicsWorldComponent world) => GetBodiesNear(GetChunkPositionFrom(worldX, worldY, world.ChunkSize), world);

    public bool IsAnythingInChunk(IntVector2 chunkPos, PhysicsWorldComponent world)
    {
        if (world.ChunkDictionary.TryGetValue(chunkPos, out var chunk))
            return !chunk.IsEmpty;
        return false;
    }

    public IEnumerable<Entity> GetBodiesNear(IntVector2 chunkPos, PhysicsWorldComponent world)
    {
        if (world.ChunkDictionary.TryGetValue(chunkPos, out var chunk))
            for (int i = 0; i < chunk.BodyCount; i++)
                yield return chunk.ContainingEntities[i];
        yield break;
    }

    private void CalculateCollisionChunks(PhysicsWorldComponent world, IEnumerable<PhysicsBodyComponent> bodies)
    {
        Profiler.StartTask("collision chunk calculation");
        var dict = world.ChunkDictionary;

        foreach (var e in dict)
            e.Value.BodyCount = 0;

        foreach (var item in bodies)
        {
            var pos = item.Collider.Bounds;

            IntVector2 min = new(
                GetChunkComponentFrom(pos.MinX, world.ChunkSize),
                GetChunkComponentFrom(pos.MinY, world.ChunkSize)
                );
            IntVector2 max = new(
                GetChunkComponentFrom(pos.MaxX, world.ChunkSize),
                GetChunkComponentFrom(pos.MaxY, world.ChunkSize)
                );

            for (int x = min.X; x <= max.X; x++)
                for (int y = min.Y; y <= max.Y; y++)
                {
                    var chunkPos = new IntVector2(x, y);
                    if (dict.TryGetValue(chunkPos, out var chunk))
                        chunk.Add(item.Entity);
                    else
                    {
                        var c = new Chunk(x, y, world.ChunkCapacity);
                        c.Add(item.Entity);
                        dict.Add(chunkPos, c);
                    }
                }
        }
        Profiler.EndTask();
    }

    private void CalculateWorldBounds(PhysicsWorldComponent world, IEnumerable<PhysicsBodyComponent> bodies)
    {
        Profiler.StartTask("world bounds calculation");
        float minX = float.MaxValue;
        float minY = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var body in bodies)
        {
            body.Collider.RecalculateBounds();
            var bounds = body.Collider.Bounds;

            minX = float.Min(bounds.MinX, minX);
            minY = float.Min(bounds.MinY, minY);

            maxX = float.Max(bounds.MaxX, maxX);
            maxY = float.Max(bounds.MaxY, maxY);

            //DebugDraw.Rectangle(new Vector2(bounds.MinX, bounds.MinY), new Vector2(bounds.Width, bounds.Height), 0, Colors.GreenYellow);
        }

        world.MaxBodyEnvelopingBounds = new Rect(minX, minY, maxX, maxY);
        Profiler.EndTask();
    }

    /// <summary>
    /// Cast a ray from a point to a normalised direction
    /// </summary>
    public bool Raycast(Vector2 origin, Vector2 direction, out RaycastResult result, float maxDistance = float.MaxValue, uint filter = uint.MaxValue, IEnumerable<Entity>? ignore = null)
    {
        result = default;

        if (!Scene.FindAnyComponent<PhysicsWorldComponent>(out var world))
        {
            Logger.Warn("PhysicsSystem without PhysicsWorld...");
            return false;
        }

        float dirLength = direction.Length();
        Vector2 normalisedDir = direction / dirLength;

        var ray = new Geometry.Ray(origin, normalisedDir);

        var worldBounds = world.MaxBodyEnvelopingBounds;
        float minStep = world.ChunkSize * .01f; //TODO dit is fout. het moet van cell-edge naar cell-edge gaan. als het zo blijft kan het mogelijk door dingen heen gaan in sommige gevallen
        //loop door alle intersections met X as en Y as, op volgorde (hoe???? help help help) van afstand
        int stepCount = 0;
        float distance = 0;

        float dX = normalisedDir.X * minStep;
        float dY = normalisedDir.Y * minStep;

        Vector2 pos = origin;
        if (isOutOfBounds())
        {
            Vector2 maxEndPoint = origin + normalisedDir * float.Min(maxDistance, 100000);
            var raySegment = new Geometry.LineSegment(origin.X, origin.Y, maxEndPoint.X, maxEndPoint.Y);
            if (Geometry.TryGetIntersection(raySegment, worldBounds, out var intersection1, out var intersection2))
            {
                float d1 = Vector2.DistanceSquared(intersection1.Value, origin);
                float d2 = Vector2.DistanceSquared(intersection2.Value, origin);
                Vector2 nearestIntersection = d1 > d2 ? intersection2.Value : intersection1.Value;
                pos.X = nearestIntersection.X;
                pos.Y = nearestIntersection.Y;

                pos.X -= dX / 2;
                pos.Y -= dY / 2;

                distance = minStep + float.Min(d1, d2);
            }
            else return false;
        }

        while (true)
        {
            stepCount++;
            if (stepCount > 100000)
            {
                Logger.Warn("Over 100000 steps");
                return false;
            }

            var currentChunk = GetChunkPositionFrom(pos.X, pos.Y, world.ChunkSize);

            if (IsAnythingInChunk(currentChunk, world))
            {
                bool hasIntersection = false;
                result = default;

                float minDistance = float.MaxValue;
                foreach (var entity in GetBodiesNear(currentChunk, world))
                {
                    if (ignore?.Contains(entity) ?? false)
                        continue;

                    if (!Scene.TryGetComponentFrom<PhysicsBodyComponent>(entity, out var bodyComponent) || !bodyComponent.PassesFilter(filter))
                        continue;

                    foreach (var intersection in bodyComponent.Collider.GetLineIntersections(ray))//TODO meerdere colliders per object??
                    {
                        var isBehindOrigin = Vector2.Dot(intersection - origin, direction) < 0;
                        if (isBehindOrigin)
                            continue;

                        if (GetChunkPositionFrom(intersection.X, intersection.Y, world.ChunkSize) != currentChunk)
                            continue;
                        float d = Vector2.DistanceSquared(intersection, origin);
                        //DebugDraw.Cross(intersection, 0.1f, Colors.Green);
                        hasIntersection = true;
                        if (d < minDistance)
                        {
                            result.Position = intersection;
                            result.Entity = entity;
                            result.Collider = bodyComponent.Collider;
                            result.Body = bodyComponent;
                            minDistance = d;
                        }
                    }
                }

                if (hasIntersection)
                {
                    var d = Vector2.Distance(origin, result.Position);
                    if (d > maxDistance)
                        return false;
                    result.Distance = d;
                    result.Normal = result.Body.Collider.SampleNormal(result.Position);
                    if (result.Body.Collider.IsPointInside(origin))
                        result.Normal *= -1;
                    return true;
                }
            }

            pos.X += dX;
            pos.Y += dY;
            distance += minStep;

            if (distance > maxDistance || isOutOfBounds())
                break;
        }

        return false;

        bool isOutOfBounds() => pos.X < worldBounds.MinX || pos.Y < worldBounds.MinY || pos.X >= worldBounds.MaxX || pos.Y >= worldBounds.MaxY;
    }

    /// <summary>
    /// Fills given array with all bodies at the given point and returns the amount (clamped to given array length)
    /// </summary>
    public int QueryPoint(Vector2 point, QueryResult[] results, uint filter = uint.MaxValue, IEnumerable<Entity>? ignore = null)
    {
        if (!Scene.FindAnyComponent<PhysicsWorldComponent>(out var world))
        {
            Logger.Warn("PhysicsSystem without PhysicsWorld...");
            return 0;
        }
        var chunkpos = GetChunkPositionFrom(point.X, point.Y, world.ChunkSize);
        if (!IsAnythingInChunk(point.X, point.Y, world))
            return 0;

        int i = 0;
        foreach (var entity in GetBodiesNear(chunkpos, world))
        {
            if (ignore?.Contains(entity) ?? false)
                continue;

            if (!Scene.TryGetComponentFrom<PhysicsBodyComponent>(entity, out var bodyComponent) ||
                bodyComponent.Collider == null || !bodyComponent.PassesFilter(filter))
                continue;

            if (bodyComponent.Collider.IsPointInside(point))
            {
                results[i++] = new QueryResult(entity, bodyComponent, bodyComponent.Collider);
                if (i >= results.Length)
                    break;
            }
        }

        return i;
    }

    /// <summary>
    /// Fills given array with all bodies at the given circle and returns the amount (clamped to given array length)
    /// </summary>
    public int QueryCircle(Vector2 center, float radius, QueryResult[] results, uint filter = uint.MaxValue, IEnumerable<Entity>? ignore = null)
    {
        if (!Scene.FindAnyComponent<PhysicsWorldComponent>(out var world))
        {
            Logger.Warn("PhysicsSystem without PhysicsWorld...");
            return 0;
        }

        int i = 0;
        float sqrdRadius = radius * radius;

        var min = GetChunkPositionFrom(center.X - radius, center.Y + radius + world.ChunkSize, world.ChunkSize);
        var max = GetChunkPositionFrom(center.X + radius + world.ChunkSize, center.Y - radius, world.ChunkSize);

        for (int x = min.X; x <= max.X; x++)
            for (int y = max.Y; y <= min.Y; y++)
            {
                var chunk = new IntVector2(x, y);
                var pos = new Vector2(x * world.ChunkSize, y * world.ChunkSize);

                if (!IsAnythingInChunk(pos.X, pos.Y, world))
                    continue;

                foreach (var entity in GetBodiesNear(pos.X, pos.Y, world))
                {
                    if (ignore?.Contains(entity) ?? false)
                        continue;

                    if (!Scene.TryGetComponentFrom<PhysicsBodyComponent>(entity, out var bodyComponent) ||
                        bodyComponent.Collider == null || !bodyComponent.PassesFilter(filter))
                        continue;

                    var nearest = bodyComponent.Collider.GetNearestPoint(center);

                    if (GetChunkPositionFrom(nearest.X, nearest.Y, world.ChunkSize) != chunk)
                        continue;

                    if (Vector2.DistanceSquared(nearest, center) <= sqrdRadius)
                    {
                        results[i++] = new QueryResult(entity, bodyComponent, bodyComponent.Collider);
                        if (i >= results.Length)
                            return i;
                    }
                }
            }

        return i;
    }

    /// <summary>
    /// Fills given array with all bodies at the given rectangle and returns the amount (clamped to given array length)
    /// </summary>
    public int QueryRectangle(Rect rectangle, ref QueryResult[] results, uint filter = uint.MaxValue)
    {
        if (!Scene.FindAnyComponent<PhysicsWorldComponent>(out var world))
        {
            Logger.Warn("PhysicsSystem without PhysicsWorld...");
            return 0;
        }

        var minX = float.Min(rectangle.MinX, rectangle.MaxX);
        var maxX = float.Max(rectangle.MinX, rectangle.MaxX);

        var minY = float.Min(rectangle.MinY, rectangle.MaxY);
        var maxY = float.Max(rectangle.MinY, rectangle.MaxY);

        rectangle = new Rect(minX, minY, maxX, maxY);

        var center = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
        int i = 0;
        var min = GetChunkPositionFrom(minX, maxY + world.ChunkSize, world.ChunkSize);
        var max = GetChunkPositionFrom(maxX + world.ChunkSize, minY, world.ChunkSize);

        for (int x = int.Min(min.X, max.X); x <= int.Max(min.X, max.X); x++)
            for (int y = int.Min(min.Y, max.Y); y <= int.Max(min.Y, max.Y); y++)
            {
                var chunk = new IntVector2(x, y);
                var pos = new Vector2(x * world.ChunkSize, y * world.ChunkSize);

                if (!IsAnythingInChunk(pos.X, pos.Y, world))
                    continue;

                foreach (var entity in GetBodiesNear(pos.X, pos.Y, world))
                {
                    if (!Scene.TryGetComponentFrom<PhysicsBodyComponent>(entity, out var bodyComponent) || 
                        bodyComponent.Collider == null || !bodyComponent.PassesFilter(filter))
                        continue;

                    var nearest = bodyComponent.Collider.GetNearestPoint(center);

                    if (GetChunkPositionFrom(nearest.X, nearest.Y, world.ChunkSize) != chunk)
                        continue;

                    if (rectangle.ContainsPoint(nearest))
                    {
                        results[i++] = new QueryResult(entity, bodyComponent, bodyComponent.Collider);
                        if (i >= results.Length)
                            return i;
                    }
                }
            }

        return i;
    }
}

public enum QueryShapeMode
{
    /// <summary>
    /// Query all bodies that intersect the shape
    /// </summary>
    Intersect,
    /// </summary>
    /// Query all bodies that are inside the shape
    /// </summary>
    Inside
}
