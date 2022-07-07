using System.Numerics;

namespace Walgelijk.Physics;

public struct RaycastResult
{
    public Entity Entity;
    public PhysicsBodyComponent Body;
    public ICollider Collider;
    public Vector2 Normal;
    public float Distance;
    public Vector2 Position;
}
