using System.Numerics;
using Walgelijk;
using Walgelijk.Physics;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct PhysicsTestScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new PhysicsSystem());
        scene.AddSystem(new PhysicsDebugSystem());
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        scene.AttachComponent(scene.CreateEntity(), new PhysicsWorldComponent() { ChunkSize = 256 });
        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#2e515b")
        });

        scene.AddSystem(new PhysicsDemoSystem());

        void addBody(ICollider collider)
        {
            var c = scene.CreateEntity();
            var t = scene.AttachComponent(c, collider.Transform);
            var b = scene.AttachComponent(c, new PhysicsBodyComponent()
            {
                BodyType = BodyType.Dynamic,
                Collider = collider,
            });
        }

        addBody(new RectangleCollider(new TransformComponent(), new Vector2(256, 128)));
        addBody(new CircleCollider(new TransformComponent() { Position = new Vector2(400, 300), Scale = new Vector2(2, 2) }, 96));
        addBody(new LineCollider(new TransformComponent() { Position = new Vector2(-400, 0) }, new Vector2(-128, 15), new Vector2(15, -200), 32));

        game.UpdateRate = 144;
        game.FixedUpdateRate = 60;
        Draw.CacheTextMeshes = -1;
        return scene;
    }

    public class PhysicsDemoSystem : Walgelijk.System
    {
        private Vector2 rayOrigin;
        private QueryResult[] buffer = new QueryResult[4];
        private HashSet<Entity> ignore = [];

        public override void FixedUpdate()
        {
            foreach (var item in Scene.GetAllComponentsOfType<PhysicsBodyComponent>())
                item.Collider.Transform.Rotation += Time.FixedInterval * 3;
        }

        public override void Update()
        {
            var phys = Scene.GetSystem<PhysicsSystem>();

            Draw.Reset();
            Draw.Colour = new Color(245, 70, 199);

            foreach (var item in Scene.GetAllComponentsOfType<PhysicsBodyComponent>())
            {
                Draw.TransformMatrix = item.Collider.Transform.LocalToWorldMatrix;
                Draw.Colour = new Color(245, 70, 199);
                if (ignore.Contains(item.Entity))
                {
                    Draw.Colour.A *= 0.5f;
                    Draw.Colour.B *= 0.5f;
                }
                switch (item.Collider)
                {
                    case CircleCollider circle:
                        Draw.Circle(default, new Vector2(circle.Radius));
                        break;
                    case RectangleCollider rect:
                        Draw.Quad(new Rect(default, rect.Size));
                        break;
                    case LineCollider rect:
                        Draw.Line(
                            rect.Start,
                            rect.End, rect.Width);
                        break;
                    default:
                        break;
                }
                Draw.ResetTransformation();
            }

            Draw.Colour = Colors.Red;
            if (phys.QueryPoint(Input.WorldMousePosition, buffer) > 0)
            {
                Draw.Circle(Input.WorldMousePosition, new Vector2(8));
                Draw.Line(Input.WorldMousePosition, Input.WorldMousePosition + buffer[0].Collider.SampleNormal(Input.WorldMousePosition) * 32, 2);

                if (Input.IsButtonPressed(MouseButton.Middle))
                {
                    var e = buffer[0].Entity;
                    if (!ignore.Remove(e))
                        ignore.Add(e);
                }
            }

            if (Input.IsButtonPressed(MouseButton.Left))
                rayOrigin = Input.WorldMousePosition;

            if (Input.IsButtonHeld(MouseButton.Left) && (Input.WorldMousePosition - rayOrigin).LengthSquared() > 1)
            {
                var rayDir = Vector2.Normalize(Input.WorldMousePosition - rayOrigin);
                Draw.Colour = Colors.Orange;

                if (phys.Raycast(rayOrigin, rayDir, out var hit, ignore: ignore))
                {
                    Draw.Line(rayOrigin, hit.Position, 1);
                    Draw.Colour = Colors.Green;
                    Draw.Line(hit.Position, hit.Position + hit.Normal * 64, 1);
                    Draw.Colour.A = 0.2f;
                    Draw.Circle(hit.Position, new Vector2(8));
                }
                else
                    Draw.Line(rayOrigin, rayOrigin + rayDir * 50000, 1);
            }
        }
    }
}
