using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
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
        /// Fired when an entity is created and registered
        /// </summary>
        public event Action<Entity> OnCreateEntity;

        /// <summary>
        /// Fired when a component is attached to an entity
        /// </summary>
        public event Action<Entity, object> OnAttachComponent;

        /// <summary>
        /// Fired when a system is added
        /// </summary>
        public event Action<ISystem> OnAddSystem;

        /// <summary>
        /// Add a system
        /// </summary>
        public void AddSystem<T>(T system) where T : ISystem
        {
            system.Scene = this;
            OnAddSystem?.Invoke(system);
            systems.Add(system.GetType(), system);
            system.Initialise();
        }

        /// <summary>
        /// Retrieve a system
        /// </summary>
        public T GetSystem<T>() where T : ISystem
        {
            return (T)systems[typeof(T)];
        }

        /// <summary>
        /// Get all systems
        /// </summary>
        public IEnumerable<ISystem> GetSystems()
        {
            return systems.Values;
        }

        /// <summary>
        /// Register a new entity to the scene
        /// </summary>
        public Entity CreateEntity()
        {
            Entity newEntity = new Entity
            {
                Identity = IdentityGenerator.Generate()
            };

            entities.Add(newEntity.Identity, newEntity);
            components.Add(newEntity, new Dictionary<Type, object>());

            OnCreateEntity?.Invoke(newEntity);

            return newEntity;
        }

        /// <summary>
        /// Get the entity struct from an entity ID. Generally not necessary
        /// </summary>
        public Entity GetEntity(int identity)
        {
            return entities[identity];
        }

        /// <summary>
        /// Get if an entity lives in the scene
        /// </summary>
        public bool HasEntity(int identity)
        {
            return entities.ContainsKey(identity);
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        public IEnumerable<Entity> GetAllEntities()
        {
            return entities.Values;
        }

        /// <summary>
        /// Get all components attached to the given entity
        /// </summary>
        public IEnumerable<object> GetAllComponentsFrom(Entity entity)
        {
            return components[entity].Values;
        }

        /// <summary>
        /// Get all components and entities of a certain type
        /// </summary>
        public IEnumerable<ComponentEntityTuple<T>> GetAllComponentsOfType<T>() where T : class
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
        public T GetComponentFrom<T>(Entity entity) where T : class
        {
            return (T)components[entity][typeof(T)];
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        public bool TryGetComponentFrom<T>(Entity entity, out T component) where T : class
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
        public bool HasComponent<T>(Entity entity) where T : struct
        {
            return components[entity].ContainsKey(typeof(T));
        }

        /// <summary>
        /// Attach a component to an entity
        /// </summary>
        public void AttachComponent<T>(Entity entity, T component) where T : class
        {
            components[entity].Add(typeof(T), component);
            OnAttachComponent?.Invoke(entity, component);
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
    public class RectangleRendererComponent
    {
        /// <summary>
        /// Colour of the rectangle
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Size of the rectangle
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// Material that is drawn with
        /// </summary>
        public Material Material { get => RenderTask.Material; set => RenderTask = new ShapeRenderTask(VertexBuffer, Matrix4x4.Identity, value); }

        /// <summary>
        /// VertexBuffer that is generated. It's best not to edit this unless you really need to.
        /// </summary>
        public VertexBuffer VertexBuffer { get; internal set; }

        /// <summary>
        /// The rendertask that is generated. It's best not to edit this unless you really need to.
        /// </summary>
        public ShapeRenderTask RenderTask { get; internal set; }
    }

    //Test shit
    public class RectangleRendererSystem : ISystem
    {
        public Scene Scene { get; set; }

        private Game game => Scene.Game;

        public void Initialise()
        {
            var rectangleRenderers = Scene.GetAllComponentsOfType<RectangleRendererComponent>();
            foreach (var pair in rectangleRenderers)
                IntialiseComponent(pair.Entity, pair.Component);

            Scene.OnAttachComponent += IntialiseComponent;
        }

        private void IntialiseComponent(Entity entity, object component)
        {
            if (!(component is RectangleRendererComponent rect)) return;
            rect.VertexBuffer = new VertexBuffer(new[] {
                new Vertex(0, 0),
                new Vertex(rect.Size.X, 0),
                new Vertex(rect.Size.X, rect.Size.Y),
                new Vertex(0, rect.Size.Y),
            })
            {
                PrimitiveType = Primitive.Quads
            };

            rect.RenderTask = new ShapeRenderTask(rect.VertexBuffer)
            {
                ModelMatrix = Matrix4x4.Identity
            };
        }

        public void Execute()
        {
            var rectangleRenderers = Scene.GetAllComponentsOfType<RectangleRendererComponent>();

            foreach (var pair in rectangleRenderers)
            {
                var rect = pair.Component;

                if (rect.RenderTask.VertexBuffer != null)
                {
                    var transform = Scene.GetComponentFrom<TransformComponent>(pair.Entity);
                    var task = rect.RenderTask;
                    task.ModelMatrix = transform.LocalToWorldMatrix;
                    game.RenderQueue.Enqueue(task);
                }
            }
        }
    }
}
