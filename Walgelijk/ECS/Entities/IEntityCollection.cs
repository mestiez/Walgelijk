using System.Collections.Generic;

namespace Walgelijk;

public interface IEntityCollection : IEnumerable<Entity>
{
    public int Count { get; }
    
    public void Add(Entity entity);
    public void Remove(Entity entity);
    public bool Has(Entity entity);
   
    public bool ClearTag(Entity entity);
    public bool HasTag(Entity entity, Tag tag);
    public void SetTag(Entity entity, Tag tag);
    public bool TryGetEntityWithTag(Tag tag, out Entity entity);
    public bool TryGetTag(Entity entity, out Tag tag);
    public IEnumerable<Entity> GetEntitiesWithTag(Tag tag);

    /// <summary>
    /// Called when a frame has ended. 
    /// The collection will empty its add and destroy buffers and update the main entity collection.
    /// </summary>
    public void SyncBuffers();
}