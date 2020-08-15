namespace Walgelijk
{
    /// <summary>
    /// Struct that holds a component and its entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ComponentEntityTuple<T> where T : class
    {
        public ComponentEntityTuple(T component, Entity entity)
        {
            Component = component;
            Entity = entity;
        }

        public T Component { get; set; }
        public Entity Entity { get; set; }
    }
}
