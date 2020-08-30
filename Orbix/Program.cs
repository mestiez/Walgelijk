using System;
using Walgelijk;
using Walgelijk.OpenTK;
using Walgelijk.NAudio;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace Orbix
{
    public class GalaxyChunkSystem : Walgelijk.System
    {
        const int StarSystemsPerChunk = 640;
        const float ChunkSize = 1000;

        private Dictionary<ChunkSpot, GalaxyChunkComponent> chunkSpots = new Dictionary<ChunkSpot, GalaxyChunkComponent>();

        public override void Initialise()
        {
            CreateChunkAt(0, 0);
            CreateChunkAt(1, 0);
            CreateChunkAt(2, 0);
            CreateChunkAt(3, 0);
        }

        private void CreateChunkAt(int x, int y)
        {
            var entity = Scene.CreateEntity();
            var chunk = new GalaxyChunkComponent();

            for (int i = 0; i < StarSystemsPerChunk; i++)
            {
                var ss = new StarSystem
                {
                    Name = $"{NameGenerator.GenerateName()} system",
                    LocalPosition = new Vector2(Utilities.RandomFloat(), Utilities.RandomFloat())
                };
                chunk.StarSystems.Add(ss);
            }

            chunk.VertexBuffer = new VertexBuffer(chunk.StarSystems.Select(s => new Vertex(s.LocalPosition.X, s.LocalPosition.Y, 0)).ToArray())
            {
                PrimitiveType = Primitive.Points
            };

            chunk.RenderTask = new ShapeRenderTask(chunk.VertexBuffer)
            {
                Material = Material.DefaultTextured
            };

            Scene.AttachComponent(entity, new TransformComponent
            {
                Scale = new Vector2(ChunkSize, ChunkSize),
                Position = new Vector2(ChunkSize * x, ChunkSize * y)
            });
            Scene.AttachComponent(entity, chunk);

            chunkSpots.Add(new ChunkSpot { x = x, y = y }, chunk);
        }

        private void RemoveChunkAt(int x, int y)
        {
            var ch = new ChunkSpot { x = x, y = y };
            if (chunkSpots.TryGetValue(ch, out var value))
            {
                chunkSpots.Remove(ch);
            }
        }

        public override void Update()
        {
            const int ext = 2;

            var cameraSystem = Scene.GetSystem<CameraSystem>();

            var transform = cameraSystem.MainCameraTransform;
            var camera = cameraSystem.MainCameraComponent;

            var offsetX = (int)MathF.Round(transform.Position.X / ChunkSize);
            var offsetY = (int)MathF.Round(transform.Position.Y / ChunkSize);

            for (int x = -ext; x < ext + 1; x++)
            {
                for (int y = -ext; y < ext + 1; y++)
                {
                    var chunk = new ChunkSpot { x = x + offsetX, y = y + offsetY };
                    if (!chunkSpots.ContainsKey(chunk))
                    {
                        CreateChunkAt(chunk.x, chunk.y);
                    }
                }
            }
        }

        public override void Render()
        {
            var chunks = Scene.GetAllComponentsOfType<GalaxyChunkComponent>();
            foreach (var pair in chunks)
                RenderChunk(pair);
        }

        private void RenderChunk(EntityWith<GalaxyChunkComponent> pair)
        {
            var ent = pair.Entity;
            var chunk = pair.Component;
            var transform = Scene.GetComponentFrom<TransformComponent>(ent);

            var task = chunk.RenderTask;
            //transform.Position = Scene.Game.Window.WindowToWorldPoint(Input.WindowMousePosition);

            task.ModelMatrix = transform.LocalToWorldMatrix;
            RenderQueue.Enqueue(task);
            //Logger.Log($"\nwndw {Input.WindowMousePosition}\nwrld {Input.WorldMousePosition}\n");
        }

        private struct ChunkSpot
        {
            public int x, y;

            public override bool Equals(object obj)
            {
                return obj is ChunkSpot spot &&
                       x == spot.x &&
                       y == spot.y;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(x, y);
            }

            public static bool operator ==(ChunkSpot left, ChunkSpot right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(ChunkSpot left, ChunkSpot right)
            {
                return !(left == right);
            }
        }
    }

    public struct StarSystem
    {
        public string Name;
        public Vector2 LocalPosition;

        public override string ToString() => Name;
    }

    public class GalaxyChunkComponent
    {
        public HashSet<StarSystem> StarSystems = new HashSet<StarSystem>();

        public VertexBuffer VertexBuffer;

        public ShapeRenderTask RenderTask;
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Game game = new Game(
                new OpenTKWindow("Orbix", default, new Vector2(1280, 720)),
                new NAudioRenderer()
                );

            game.Window.TargetFrameRate = 120;
            game.Window.TargetUpdateRate = 60;
            game.Window.VSync = false;
            game.Window.RenderTarget.ClearColour = new Color("#000000");

            game.Scene = CreateGameScene();

            game.Start();
        }

        private static Scene CreateGameScene()
        {
            Scene scene = new Scene();

            var camera = scene.CreateEntity();
            scene.AttachComponent(camera, new TransformComponent());
            scene.AttachComponent(camera, new CameraComponent());

            CreateDebugGrid(scene, 4, 4, 10);

            scene.AddSystem(new GalaxyChunkSystem());
            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new DebugCameraSystem { Speed = 0.1f });
            scene.AddSystem(new ShapeRendererSystem());
            scene.AddSystem(new CameraSystem());

            return scene;
        }

        private static void CreateDebugGrid(Scene scene, int width = 8, int height = 8, int step = 10)
        {
            int xExtent = width / 2;
            int yExtent = height / 2;

            for (int y = -yExtent; y < yExtent + 1; y++)
            {
                for (int x = -xExtent; x < xExtent + 1; x++)
                {
                    var point = scene.CreateEntity();
                    var pos = new Vector2(x, y) * step;
                    scene.AttachComponent(point, new TransformComponent { Scale = new Vector2(0.04f, 0.04f), Position = pos });
                    scene.AttachComponent(point, new TextComponent($"{pos.X}, {pos.Y}"));

                    var dot = scene.CreateEntity();
                    scene.AttachComponent(dot, new TransformComponent { Scale = new Vector2(0.1f, 0.1f), Position = pos });
                    scene.AttachComponent(dot, new RectangleShapeComponent());
                }
            }
        }
    }
}
