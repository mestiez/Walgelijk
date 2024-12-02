using koploper;
using System.Numerics;
using Walgelijk.AssetManager;
using Walgelijk.Onion;
using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Prism.Editor;

public static class MapEditorScene
{
    private static Scene? scene;

    /// <summary>
    /// Load the map editor scene (persistent).
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    public static Scene LoadScene(Game game)
    {
        scene ??= CreateEditorScene(game); // create scene if null
        game.UpdateRate = 120;
        return scene;
    }

    private static Scene CreateEditorScene(Game game)
    {
        var scene = new Scene(game);
        scene.ScenePersistence = ScenePersistence.Persist;

        scene.AddSystem(new EditorSystem());
        scene.AddSystem(new PrismTransformSystem());
        scene.AddSystem(new PrismCameraSystem());
        scene.AddSystem(new PrismFreecamSystem());
        scene.AddSystem(new PrismMeshRendererSystem());

        scene.AddSystem(new OnionSystem());

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new PrismTransformComponent());
        scene.AttachComponent(camera, new PrismCameraComponent
        {
            ClearColour = new Color(0x2e021b)
        });

        var editor = scene.AttachComponent(scene.CreateEntity(), new EditorDataComponent());
        scene.AttachComponent(scene.CreateEntity(), new MeshProviderComponent());

        return scene;
    }
}

public class EditorDataComponent : Component
{
    
}

public struct NewEntityArgs
{
    public Scene Scene;
    public Vector3 Position;
}

public abstract class EntityProviderComponent : Component
{
    public abstract string Label { get; }
    public abstract Entity Create(in NewEntityArgs args);
}

public class MapSelectableComponent : Component
{
    public Cuboid BoundingBox;
}

public class MeshProviderComponent : EntityProviderComponent
{
    public override string Label => "Mesh";

    public override Entity Create(in NewEntityArgs args)
    {
        var scene = args.Scene;
        var entity = scene.CreateEntity();

        var transform = scene.AttachComponent(entity, new PrismTransformComponent
        {
            Position = args.Position,
            Scale = new Vector3(1, 1, 1)
        });

        MeshPrimitives.GenerateCenteredCube(1, out var verts, out var inds);

        var mesh = scene.AttachComponent(entity, new PrismMeshComponent(
            verts, inds, new Material(Material.DefaultTextured) { DepthTested = true }));

        return entity;
    }
}

public class EditorSystem : Walgelijk.System
{
    public override void Update()
    {
        Ui.Layout.StickLeft().StickTop().Size(200, 500).VerticalLayout();
        Ui.StartScrollView();
        {
            int i = 0;
            foreach (var entityProvider in Scene.GetAllComponentsOfType<EntityProviderComponent>())
            {
                Ui.Layout.FitWidth().Height(32).StickLeft();
                if (Ui.Button(entityProvider.Label, identity: i++))
                {
                    entityProvider.Create(new NewEntityArgs
                    {
                        Scene = Scene,
                        Position = Utilities.RandomPointInCircle().XXY() with { Y = 0 }
                    });
                }
            }
        }
        Ui.End();
    }
}