using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Walgelijk;

public interface IComponentCollection
{    
    void Add(object component, Entity entity);
    bool DeleteEntity(Entity entity);
    IEnumerable GetAllComponentsFrom(Entity entity);
    IEnumerable<EntityWith<T>> GetAllComponentsOfType<T>() where T : class;
    T GetComponentFrom<T>(Entity entity) where T : class;
    bool GetEntityFromComponent(object comp, out Entity entity);
    bool RemoveComponentOfType(Type type, Entity entity);
    bool RemoveComponentOfType<T>(Entity entity);
    bool TryGetComponentFrom<T>(Entity entity, [NotNullWhen(true)] out T? component) where T : class;

    ComponentDisposalManager DisposalManager { get; }
}