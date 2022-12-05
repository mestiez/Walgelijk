using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Walgelijk;

public class BasicEntityCollection : IEntityCollection
{
    private readonly HashSet<Entity> entities = new();
    private readonly ConcurrentDictionary<Entity, Tag> tagByEntity = new();
    private readonly ConcurrentDictionary<Tag, HashSet<Entity>> entitiesByTag = new();

    public int Count => entities.Count;

    /// <summary>
    /// Attaches a tag to an entity
    /// </summary>
    public void SetTag(Entity entity, Tag tag)
    {
        if (tagByEntity.ContainsKey(entity))//does this entity already have a tag? 
        {
            if (entitiesByTag.TryGetValue(tag, out var existing))
                existing.Remove(entity);//remove entity from old tag collection

            tagByEntity[entity] = tag;
        }
        else
        {
            //this entity never had a tag so we need to create everything
            tagByEntity.TryAdd(entity, tag);

            if (entitiesByTag.TryGetValue(tag, out var coll))//does the tag already have a collection?
                coll.Add(entity);
            else
                entitiesByTag.TryAdd(tag, new HashSet<Entity> { entity });
        }
    }

    /// <summary>
    /// Returns true if the entity has the given tag
    /// </summary>
    public bool HasTag(Entity entity, Tag tag) => tagByEntity.TryGetValue(entity, out var t) && t == tag;

    /// <summary>
    /// Detaches a tag from an entity
    /// </summary>
    public bool ClearTag(Entity entity)
    {
        if (tagByEntity.Remove(entity, out var tag))
        {
            var coll = entitiesByTag[tag];
            bool s = coll.Remove(entity);
            if (coll.Count == 0)
                s &= entitiesByTag.Remove(tag, out _);
            return s;
        }
        return false;
    }

    /// <summary>
    /// Returns true if the given entity has a tag attached. The tag is returned in the output parameter <paramref name="tag"/>
    /// </summary>
    public bool TryGetTag(Entity entity, out Tag tag) => tagByEntity.TryGetValue(entity, out tag);

    /// <summary>
    /// Returns all entities with the given tag
    /// </summary>
    public IEnumerable<Entity> GetEntitiesWithTag(Tag tag)
    {
        if (entitiesByTag.TryGetValue(tag, out var coll))
            return coll;
        return Array.Empty<Entity>();
    }

    /// <summary>
    /// Returns true if any entity with a given tag is found. The entity is returned in the output parameter <paramref name="entity"/>
    /// </summary>
    public bool TryGetEntityWithTag(Tag tag, out Entity entity)
    {
        if (entitiesByTag.TryGetValue(tag, out var coll) && coll.Count > 0)
        {
            entity = coll.First();
            return true;
        }
        entity = default;
        return false;
    }

    public IEnumerator<Entity> GetEnumerator() => entities.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => entities.GetEnumerator();

    public void Add(Entity entity) => entities.Add(entity);
    public void Remove(Entity entity)
    {
        if (tagByEntity.Remove(entity, out var tag))
            if (entitiesByTag.TryGetValue(tag, out var set))
                set.Remove(entity);
        entities.Remove(entity);
    }

    public bool Has(Entity entity) => entities.Contains(entity);

    public void SyncBuffers()
    {
    }
}
