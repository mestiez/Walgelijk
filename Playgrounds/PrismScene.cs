using NAudio.Wave;
using System.Numerics;
using Walgelijk;
using Walgelijk.Prism;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct PrismScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);

        scene.AddSystem(new SpinnySystem());

        scene.AddSystem(new PrismTransformSystem());
        scene.AddSystem(new PrismCameraSystem());
        scene.AddSystem(new PrismFreecamSystem());
        scene.AddSystem(new PrismMeshRendererSystem());

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new PrismTransformComponent());
        scene.AttachComponent(camera, new PrismCameraComponent
        {
            ClearColour = new Color("#152123")
        });

        MeshPrimitives.GenerateCenteredCube(1, out var verts, out var indices);
        var vxb = new VertexBuffer(verts, indices) { PrimitiveType = Primitive.Triangles };

        for (int i = 0; i < 15; i++)
        {
            var cube = scene.CreateEntity();
            scene.AttachComponent(cube, new PrismTransformComponent
            {
                Scale = new Vector3(Utilities.RandomFloat(0.5f, 1)),
                Rotation = Quaternion.CreateFromYawPitchRoll(Utilities.RandomFloat(), Utilities.RandomFloat(), Utilities.RandomFloat()),
                Position = new Vector3(Utilities.RandomFloat(-20, 20), Utilities.RandomFloat(0, 20), Utilities.RandomFloat(-20, 20))
            });
            var vv = scene.AttachComponent(cube, new PrismMeshComponent(vxb, new Material(Material.DefaultTextured) { DepthTested = true }));
            vv.Material.SetUniform("mainTex", TexGen.Colour(1, 1, Utilities.RandomColour()));
            scene.AttachComponent(cube, new SpinnyComponent());
        }

        MeshPrimitives.GenerateQuad(new Vector2(1), out verts, out indices);
        vxb = new VertexBuffer(verts, indices) { PrimitiveType = Primitive.Triangles };

        for (int x = -5; x < 10; x++)
            for (int y = -5; y < 10; y++)
            {
                var quad = scene.CreateEntity();
                scene.AttachComponent(quad, new PrismTransformComponent
                {
                    Scale = new Vector3(0.5f),
                    Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2),
                    Position = new Vector3(x, -0.5f, y)
                });

                scene.AttachComponent(quad, new PrismMeshComponent(vxb, new Material(Material.DefaultTextured) { DepthTested = true })).Material.SetUniform("mainTex", TexGen.Colour(1, 1, Utilities.RandomColour()));
            }

        {
            var cat = scene.CreateEntity();
            scene.AttachComponent(cat, new SpinnyComponent());
            scene.AttachComponent(cat, new PrismTransformComponent
            {
                Scale = new Vector3(1),
                Rotation = Quaternion.CreateFromYawPitchRoll(0, MathF.PI / -2, 0),
                Position = new Vector3(0, 5, 0)
            });

            //var m = MeshLoader.Load("resources/meshes/vase.dae")[0];
            //scene.AttachComponent(cat, new PrismMeshComponent(m.Vertices, m.Indices, m.Material ?? new Material(Material.DefaultTextured) { DepthTested = true }));
        }

        return scene;
    }

    public class SpinnyComponent : Component { }

    public class SpinnySystem : Walgelijk.System
    {
        public override void Update()
        {
            foreach (var item in Scene.GetAllComponentsOfType<SpinnyComponent>())
            {
                var transform = Scene.GetComponentFrom<PrismTransformComponent>(item.Entity);
                transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Time.DeltaTime);
            }
        }
    }
}
