using Walgelijk;
using Walgelijk.Imgui;
using Walgelijk.Onion;
using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public struct IMGUIScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        Gui.SetCursorStack = false;
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new GuiSystem());
        scene.AddSystem(new OnionSystem());
        scene.AddSystem(new IMGUITestSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#550055")
        });
        game.UpdateRate = 60;
        return scene;
    }

    public class IMGUITestSystem : Walgelijk.System
    {
        public static string[] DropdownOptions =
        {
            "Amsterdam",
            "Rotterdam",
            "Den Haag",
            "Utrecht",
            "Eindhoven",
            "Tilburg",
            "Groningen",
            "Breda",
            "Nijmegen",
            "Apeldoorn"
        };

        public static int DropdownSelectedIndex;

        private const string textRectContents =
            "This paper presents a software capable of visualizing large amounts of Diffusion MRI tractography data. " +
            "The application can render three-dimensional streamlines to represent white matter tracts inside the human brain, and provide tools for exploring and investigating the 3D visualization in real-time. " +
            "It also provides several methods of visualization that are suited for various investigative purposes, including interactivity such as camera movement (moving, rotating, dragging and zooming) " +
            "and visualizing the streamlines intersecting with an arbitrary sphere. To facilitate this, the software must be able to read TCK files, perform preprocessing tasks, " +
            "apply domain specific filters and tooling for analysis, use signed distance fields for rendering, generate curved tubes meshes, utilize GPU raymarching and raycasting, " +
            "use simple shading techniques appropriate for this context, employ voxels and raycasting to reinterpret the data to make it less costly to render, perform line culling to reduce occluded lines, " +
            "utilize instanced rendering of meshes using vertex attributes, and implement level of detail rendering.";

        public override void Initialise()
        {
            //Onion.Theme.Font = Resources.Load<Font>("cambria.fnt");
        }

        public override void Update()
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer - 1);
            Draw.Colour = Onion.Theme.Background.Color;
            Draw.Quad(new Rect(0, 0, Window.Width, Window.Height));

            {
                //Onion.Layout.Offset(Onion.Theme.Padding, Onion.Theme.Padding);
                Onion.Layout.Size(128, Window.Height / 2);
                Onion.Layout.VerticalLayout();

                Onion.Tree.Start(75, new ScrollView());
                for (int i = 0; i < 12; i++)
                {
                    Onion.Layout.Offset(Onion.Theme.Padding, Onion.Theme.Padding);
                    Onion.Layout.Size(0, 32);
                    Onion.Layout.FitContainer(1, null);
                    if (Walgelijk.Onion.Controls.Button.Click("Ik besta ook " + i, i))
                        Audio.PlayOnce(Sound.Beep);
                }
                Onion.Tree.End();
            }

            {
                const float hotbarHeight = 64;
                Onion.Layout.FitContainer(1, 0);
                Onion.Layout.Height(hotbarHeight);
                Onion.Layout.CenterHorizontal();
                Onion.Layout.Offset(0, Window.Height - hotbarHeight - Onion.Theme.Padding);

                Onion.Tree.Start(106, new ScrollView());
                for (int i = 0; i < 24; i++)
                {
                    Onion.Layout.Size(hotbarHeight - Onion.Theme.Padding * 2, hotbarHeight - Onion.Theme.Padding * 2);
                    Onion.Layout.CenterVertical();
                    Onion.Layout.Offset(Onion.Theme.Padding + i * (hotbarHeight - Onion.Theme.Padding), 0);
                    if (Walgelijk.Onion.Controls.ImageButton.Click(Texture.ErrorTexture, ImageContainmentMode.Cover, i))
                        Audio.PlayOnce(Sound.Beep);
                }
                Onion.Tree.End();
            }

            {
                Onion.Layout.Size(128, 24);
                Onion.Layout.Offset(128, Onion.Theme.Padding);
                Dropdown<string>.Create(DropdownOptions, ref DropdownSelectedIndex);
            }

            {
                Onion.Layout.Size(150, 24);
                Onion.Layout.Offset(128 + 128 + Onion.Theme.Padding, Onion.Theme.Padding);
                var s = Scene.GetSystem<OnionSystem>();
                Dropdown<UiDebugOverlay>.CreateForEnum(ref s.DebugOverlay);
            }

            Onion.Layout.Size(128 + 150, 256);
            Onion.Layout.Offset(128, 24 + Onion.Theme.Padding);

            TextRect.Create(textRectContents, HorizontalTextAlign.Left, VerticalTextAlign.Top);
        }

        public override void Render()
        {

        }
    }
}
