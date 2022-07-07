namespace Walgelijk.Physics;

public struct QueryResult
{
    public readonly Entity Entity;
    public readonly PhysicsBodyComponent Body;
    public readonly ICollider Collider;

    public QueryResult(Entity entity, PhysicsBodyComponent body, ICollider collider)
    {
        Entity = entity;
        Body = body;
        Collider = collider;
    }
}
