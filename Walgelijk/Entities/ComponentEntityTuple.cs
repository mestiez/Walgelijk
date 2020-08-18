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

        /// <summary>
        /// The component of type <see cref="T"/>
        /// </summary>
        public T Component { get; set; }
        /// <summary>
        /// The entity that <see cref="Component"/> is attached to
        /// </summary>
        public Entity Entity { get; set; }
    }
}
