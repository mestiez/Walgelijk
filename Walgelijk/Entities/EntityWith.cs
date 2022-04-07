namespace Walgelijk
{
    /// <summary>
    /// Struct that holds a component and its entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct EntityWith<T> where T : class
    {
        public EntityWith(T component, Entity entity)
        {
            Component = component;
            Entity = entity;
        }

        /// <summary>
        /// The component of type <typeparamref name="T"/>
        /// </summary>
        public T Component { get; set; }
        /// <summary>
        /// The entity that <see cref="Component"/> is attached to
        /// </summary>
        public Entity Entity { get; set; }
    }
}
