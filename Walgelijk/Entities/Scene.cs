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

        private readonly Dictionary<Entity, List<object>> components = new Dictionary<Entity, List<object>>();
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
            components.Add(newEntity, new List<object>());
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
            return components[entity];
        }

        /// <summary>
        /// Get all components and entities of a certain type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<ComponentEntityTuple<T>> GetAllComponentsOfType<T>()
        {
            foreach (var componentCollection in components)
                foreach (var component in componentCollection.Value)
                    if (component is T typedComponent)
                        yield return new ComponentEntityTuple<T>(typedComponent, componentCollection.Key);
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="entity">Entity ID or entity struct</param>
        /// <returns></returns>
        public T GetComponentFrom<T>(Entity entity)
        {
            var allComponents = GetAllComponentsFrom(entity);
            foreach (var component in allComponents)
                if (component is T result)
                    return result;

            return default;
        }

        /// <summary>
        /// Attach a component to an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        public void AttachComponent<T>(Entity entity, T component) where T : struct
        {
            components[entity].Add(component);
        }

        /// <summary>
        /// Executes all systems. This is typically handled by the window implementation
        /// </summary>
        public void ExecuteSystems()
        {
            foreach (var system in systems.Values)
                system.Execute();
        }
    }

    /// <summary>
    /// Struct that holds a component and its entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ComponentEntityTuple<T>
    {
        public ComponentEntityTuple(T component, Entity entity)
        {
            Component = component;
            Entity = entity;
        }

        public T Component { get; set; }
        public Entity Entity { get; set; }
    }

    /// <summary>
    /// Holds game logic
    /// </summary>
    public interface ISystem
    {
        /// <summary>
        /// Containing scene
        /// </summary>
        public Scene Scene { get; set; }

        /// <summary>
        /// Run the logic
        /// </summary>
        public void Execute();
    }

    /// <summary>
    /// An entity. Does nothing, simply holds an identity. Implicitly an integer.
    /// </summary>
    public struct Entity
    {
        /// <summary>
        /// The identity of the entity
        /// </summary>
        public int Identity { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Entity entity &&
                   Identity == entity.Identity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identity);
        }

        public static bool operator ==(Entity left, Entity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }

        public static implicit operator int(Entity entity)
        {
            return entity.Identity;
        }

        public override string ToString()
        {
            return $"Entity {Identity}";
        }
    }

    /// <summary>
    /// Generates entity identities
    /// </summary>
    public struct IdentityGenerator
    {
        private static int lastIdentity = -1;

        public static int Generate()
        {
            lastIdentity++;
            return lastIdentity;
        }
    }

    /// <summary>
    /// Basic component that holds transformation data
    /// </summary>
    public struct TransformComponent
    {
        /// <summary>
        /// Position of the entity in world space
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Rotation in degrees of the entity in world space
        /// </summary>
        public float Rotation { get; set; }
    }

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
    }

    public class RectangleRendererSystem : ISystem
    {
        public Scene Scene { get; set; }

        private Game game => Scene.Game;

        public void Execute()
        {
            var components = Scene.GetAllComponentsOfType<RectangleRendererComponent>();
            float x = MathF.Sin((float)DateTime.Now.TimeOfDay.TotalSeconds);
            foreach (var c in components)
            {
                game.RenderQueue.Enqueue(new ImmediateRenderTask(new[] {
                new Vertex(x-1, x-1),
                new Vertex(x-1, x),
                new Vertex(x, x),
                new Vertex(x, x-1),
                }, Primitive.LineLoop));
            }
        }
    }
}
