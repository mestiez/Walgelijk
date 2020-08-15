using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Stores and manages components and systems
    /// </summary>
    public sealed class Scene
    {
        /// <summary>
        /// Game this scene belongs to
        /// </summary>
        public Game Game { get; internal set; }

        private readonly Dictionary<Entity, Dictionary<Type, object>> components = new Dictionary<Entity, Dictionary<Type, object>>();
        private readonly Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        private readonly Dictionary<Type, ISystem> systems = new Dictionary<Type, ISystem>();

        /// <summary>
        /// Add a system
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="system"></param>
        public void AddSystem<T>(T system) where T : ISystem
        {
            system.Scene = this;
            systems.Add(system.GetType(), system);
        }

        /// <summary>
        /// Retrieve a system
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSystem<T>() where T : ISystem
        {
            return (T)systems[typeof(T)];
        }

        /// <summary>
        /// Get all systems
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ISystem> GetSystems()
        {
            return systems.Values;
        }

        /// <summary>
        /// Register a new entity to the scene
        /// </summary>
        /// <returns></returns>
        public Entity CreateEntity()
        {
            Entity newEntity = new Entity
            {
                Identity = IdentityGenerator.Generate()
            };

            entities.Add(newEntity.Identity, newEntity);
            components.Add(newEntity, new Dictionary<Type, object>());
            return newEntity;
        }

        /// <summary>
        /// Get the entity struct from an entity ID. Generally not necessary
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public Entity GetEntity(int identity)
        {
            return entities[identity];
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Entity> GetAllEntities()
        {
            return entities.Values;
        }

        /// <summary>
        /// Get all components attached to the given entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<object> GetAllComponentsFrom(Entity entity)
        {
            return components[entity].Values;
        }

        /// <summary>
        /// Get all components and entities of a certain type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<ComponentEntityTuple<T>> GetAllComponentsOfType<T>() where T : struct
        {
            foreach (var pair in components)
            {
                var componentDictionary = pair.Value;
                var entity = pair.Key;

                if (componentDictionary.TryGetValue(typeof(T), out object component))
                    yield return new ComponentEntityTuple<T>((T)component, entity);
            }
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="entity">Entity ID or entity struct</param>
        /// <returns></returns>
        public T GetComponentFrom<T>(Entity entity) where T : struct
        {
            return (T)components[entity][typeof(T)];
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="entity">Entity ID or entity struct</param>
        /// <returns></returns>
        public bool TryGetComponentFrom<T>(Entity entity, out T component) where T : struct
        {
            component = default;
            if (components[entity].TryGetValue(typeof(T), out object c))
            {
                component = (T)c;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get if an entity has a component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool HasComponent<T>(Entity entity) where T : struct
        {
            return components[entity].ContainsKey(typeof(T));
        }

        /// <summary>
        /// Attach a component to an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        public void AttachComponent<T>(Entity entity, T component) where T : struct
        {
            components[entity].Add(typeof(T), component);
        }

        /// <summary>
        /// Executes all systems. This is typically handled by the window implementation
        /// </summary>
        public void ExecuteSystems()
        {
            foreach (var system in systems.Values)
                system.Execute();
        }

        //TODO voeg manier to om meerdere componenten te krijgen per keer
    }

    //Test shit
    public struct RectangleRendererComponent
    {
        /// <summary>
        /// Colour of the rectangle
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Size of the rectangle
        /// </summary>
        public Vector2 Size { get; set; }

        public float offset;
        public float speed;
    }

    //Test shit
    public class RectangleRendererSystem : ISystem
    {
        public Scene Scene { get; set; }

        private Game game => Scene.Game;

        public void Execute()
        {
            var rectangleRenderers = Scene.GetAllComponentsOfType<RectangleRendererComponent>();

            float t = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            foreach (var pair in rectangleRenderers)
            {
                //float x = s * pair.Component.Size.X;
                //float y = s * pair.Component.Size.Y;

                var rect = pair.Component;
                float offset = MathF.Sin(t * rect.speed + rect.offset);

                var transform = Scene.GetComponentFrom<TransformComponent>(pair.Entity);
                float x = transform.Position.X + offset;
                float y = transform.Position.Y + offset;

                game.RenderQueue.Enqueue(new ImmediateRenderTask(new[] {
                    new Vertex(x - rect.Size.X/2 ,y - rect.Size.Y/2),
                    new Vertex(x + rect.Size.X/2, y - rect.Size.Y/2),
                    new Vertex(x + rect.Size.X/2, y + rect.Size.Y/2),
                    new Vertex(x - rect.Size.X/2 ,y + rect.Size.Y/2),
                }, Primitive.Quads));
            }
        }
    }
}
