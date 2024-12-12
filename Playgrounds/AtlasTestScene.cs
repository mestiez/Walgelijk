using System.Numerics;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.Onion;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct AtlasTestScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TestSystem());
        scene.AddSystem(new OnionSystem());
        return scene;
    }

    public class TestSystem : Walgelijk.System
    {
#pragma warning disable TEXTUREATLAS 
        private TextureAtlas atlas = new();
#pragma warning restore TEXTUREATLAS 

        public override void Update()
        {
            Draw.Reset();
            Draw.Order = RenderOrder.Bottom;
            Draw.Clear(Colors.Black);

            Draw.Order = RenderOrder.Zero;
            Draw.ScreenSpace = true;

            Draw.Image(atlas.Page, new Rect(0, 0, Window.Width, Window.Height).Expand(-100), ImageContainmentMode.Contain);

            if (Input.IsButtonPressed(MouseButton.Left))
                atlas.Build(RenderQueue);

            Ui.Layout.Size(120, Window.Height).VerticalLayout();
            Ui.StartGroup(false);
            {
                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                if (Ui.Button("Add"))
                {
                    var s = Utilities.PickRandom(Assets.EnumerateFolder("sprites"));
                    atlas.Add(Assets.Load<Texture>(s).Value, s.ToNamedString());
                }

                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                Ui.Button("Clear");

                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                Ui.Button("Build");
            }
            Ui.End();
        }
    }
}
