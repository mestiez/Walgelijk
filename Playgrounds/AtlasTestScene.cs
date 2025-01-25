using System.Numerics;
using System.Reflection;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.Onion;
using Walgelijk.Onion.Controls;
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
        private IBinPacker[] Packers = Assembly.GetAssembly(typeof(IBinPacker))!
            .GetTypes().Where(t => t.IsAssignableTo(typeof(IBinPacker)) && t.IsClass)
            .Select(Activator.CreateInstance).Cast<IBinPacker>().ToArray();

#pragma warning disable TEXTUREATLAS 
        private TextureAtlas atlas = new()
        {
            BinPacker = new ShelfBinPacker()
        };
#pragma warning restore TEXTUREATLAS 

        private Texture checkerboard = TexGen.Checkerboard(16, 16, 8, Colors.White, Colors.Gray.Brightness(1.5f));

        public override void Update()
        {
            Draw.Reset();
            Draw.Order = RenderOrder.Bottom;
            Draw.Clear(new Color(25, 25, 25));

            Draw.Order = RenderOrder.Zero;
            Draw.ScreenSpace = true;

            var imgRect = Draw.Image(atlas.Page, new Rect(0, 0, Window.Width, Window.Height).Expand(-100), ImageContainmentMode.Contain);

            Draw.ResetTexture();
            Draw.Colour = Colors.White;
            Draw.Texture = checkerboard;
            Draw.ImageMode = ImageMode.Tiled;
            Draw.Order = new RenderOrder(-1, 0);
            Draw.Quad(imgRect);
            Draw.Order = default;

            Draw.ResetTexture();
            foreach (var item in atlas.Entries)
            {
                var uv = atlas.GetUvRect(item);

                var rect = new Rect(
                    Utilities.MapRange(0, 1, imgRect.MinX, imgRect.MaxX, uv.X),
                    Utilities.MapRange(1, 0, imgRect.MinY, imgRect.MaxY, uv.Y),
                    Utilities.MapRange(0, 1, imgRect.MinX, imgRect.MaxX, uv.Z),
                    Utilities.MapRange(1, 0, imgRect.MinY, imgRect.MaxY, uv.W)
                ).SortComponents();

                Draw.Colour = Colors.Transparent;
                Draw.OutlineColour = Colors.Red;
                Draw.OutlineWidth = 1;
                if (rect.ContainsPoint(Input.WindowMousePosition))
                {
                    Draw.Colour = Colors.White;
                    Draw.Text(item, new Vector2(Window.Width * 0.5f, Window.Height - 10), Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Bottom);
                    Draw.OutlineWidth = 2;
                    Draw.Colour = Colors.Red.WithAlpha(0.1f);
                    Draw.FontSize = 12;
                }

                Draw.Quad(rect);
            }

            Draw.Colour = Colors.White;
            Draw.Text($"{atlas.Page.Width}x{atlas.Page.Height}", new Vector2(Window.Width * 0.5f, 10), Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Top);

            DrawUi();
        }

        private void DrawUi()
        {
            Ui.Layout.Size(250, Window.Height).VerticalLayout();
            Ui.StartGroup(false);
            {
                int i = 0;
                foreach (var item in Packers)
                {
                    Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                    if (item == atlas.BinPacker)
                        Ui.Theme.ForegroundColor(Colors.Green.Brightness(0.8f).Saturation(0.4f)).Once();
                    if (Ui.Button(item.GetType().Name, identity: i++))
                    {
                        atlas.Build(RenderQueue);
                        atlas.BinPacker = item;
                    }
                }
                Ui.Spacer(10);
                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                if (Ui.Button("Add"))
                {
                    if (Input.IsKeyHeld(Key.LeftShift))
                    {
                        foreach (var s in Assets.EnumerateFolder("sprites"))
                            atlas.Add(Assets.Load<Texture>(s).Value, s.ToNamedString());
                    }
                    else
                    {
                        var s = Utilities.PickRandom(Assets.EnumerateFolder("sprites"));
                        int x = 0;
                        while (x < 1024)
                        {
                            s = Utilities.PickRandom(Assets.EnumerateFolder("sprites"));
                            if (!atlas.Entries.Contains(s.ToNamedString()))
                                break;
                            x++;
                        }
                        atlas.Add(Assets.Load<Texture>(s).Value, s.ToNamedString());
                    }
                    atlas.Build(RenderQueue);
                }

                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                if (Ui.Button("Clear"))
                {
                    atlas.Clear();
                    atlas.Build(RenderQueue);
                }

                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                if (Ui.Button("Build"))
                    atlas.Build(RenderQueue);

                Ui.Spacer(10);
                Ui.Label("W");
                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                if (Ui.IntStepper(ref atlas.MaxWidth, (256, int.MaxValue)))
                    atlas.Build(RenderQueue);
                Ui.Spacer(10);
                Ui.Label("H");
                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                if (Ui.IntStepper(ref atlas.MaxHeight, (256, int.MaxValue)))
                    atlas.Build(RenderQueue);

                Ui.Spacer(10);
                Ui.Label("Padding");
                Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                if (Ui.IntStepper(ref atlas.Padding, (0, 20)))
                    atlas.Build(RenderQueue);

                if (atlas.BinPacker is GuillotineBinPacker guillotine)
                {
                    Ui.Spacer(0);
                    Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                    if (Ui.EnumDropdown(ref guillotine.Mode))
                        atlas.Build(RenderQueue); Ui.Spacer(0);
                    Ui.Layout.FitWidth(true).Height(30).CenterHorizontal();
                    if (Ui.EnumDropdown(ref guillotine.Direction))
                        atlas.Build(RenderQueue);
                }
            }
            Ui.End();
        }
    }
}
