namespace Walgelijk.Physics
{
    /// <summary>
    /// Physics body component. Contains all data that describes a static or dynamic physical body
    /// </summary>
    [RequiresComponents(typeof(TransformComponent))]
    public class PhysicsBodyComponent : Component
    {
        public uint FilterBits = 0b_0000_0000_0000_0001;
        public ICollider Collider;
        public BodyType BodyType;

        public bool PassesFilter(uint filter) => (filter & FilterBits) != 0;
    }
}
