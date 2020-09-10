using System.Linq;

namespace Walgelijk
{
    /// <summary>
    /// Structure that holds an entity and its components. Meant for serialisation.
    /// </summary>
    public struct Prefab
    {
        /// <summary>
        /// The entity
        /// </summary>
        public Entity EntityID;
        /// <summary>
        /// Attached components
        /// </summary>
        public object[] Components;

        /// <summary>
        /// Construct a prefab
        /// </summary>
        public Prefab(Entity entityID, object[] components)
        {
            EntityID = entityID;
            Components = components;
        }

        /// <summary>
        /// Construct a prefab from an existing entity
        /// </summary>
        public Prefab(Entity entityID, Scene scene)
        {
            EntityID = entityID;
            Components = scene.GetAllComponentsFrom(entityID).ToArray();
        }

        /// <summary>
        /// Unpack the prefab in the scene. This <b>will ignore the stored <see cref="EntityID"/></b> and assign a new ID instead
        /// </summary>
        public void CopyTo(Scene scene)
        {
            var entity = scene.CreateEntity();

            foreach (object component in Components)
                scene.AttachComponent(entity, component);
        }

        /// <summary>
        /// Unpack the prefab in the scene. This will keep the stored ID and will fail when the ID collides.
        /// </summary>
        public void UnpackTo(Scene scene)
        {
            scene.RegisterExistingEntity(EntityID);

            foreach (object component in Components)
                scene.AttachComponent(EntityID, component);
        }
    }
}
