using Walgelijk;
using Walgelijk.Imgui;
using Walgelijk.Onion;
using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;
using Button = Walgelijk.Onion.Controls.Button;

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
        game.UpdateRate = 144;
        Draw.CacheTextMeshes = -1;
        return scene;
    }

    public class IMGUITestSystem : Walgelijk.System
    {
        public readonly static string[] DropdownOptions =
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
        private static bool PasswordCheckbox = true;

        public readonly static bool[] WindowsOpen = { true, true };

        private string textRectContents =
            "This paper presents a software capable of visualizing large amounts of Diffusion MRI tractography data. " +
            "The application can render three-dimensional streamlines to represent white matter tracts inside the human brain, and provide tools for exploring and investigating the 3D visualization in real-time. " +
            "It also provides several methods of visualization that are suited for various investigative purposes, including interactivity such as camera movement (moving, rotating, dragging and zooming) " +
            "and visualizing the streamlines intersecting with an arbitrary sphere. To facilitate this, the software must be able to read TCK files, perform preprocessing tasks, " +
            "apply domain specific filters and tooling for analysis, use signed distance fields for rendering, generate curved tubes meshes, utilize GPU raymarching and raycasting, " +
            "use simple shading techniques appropriate for this context, employ voxels and raycasting to reinterpret the data to make it less costly to render, perform line culling to reduce occluded lines, " +
            "utilize instanced rendering of meshes using vertex attributes, and implement level of detail rendering.";

        private string textBoxContent = "This is the only reliable way to contact us.";

        public override void Initialise()
        {
            //Onion.Theme.FontSize = 12;
            //Onion.Theme.Font = Resources.Load<Font>("inter-tight.fnt");
        }

        public override void Update()
        {
            var layout = Onion.Layout;
            var gui = Onion.Tree;

            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer - 1);
            Draw.Colour = Onion.Theme.Background.Color;
            Draw.Quad(new Rect(0, 0, Window.Width, Window.Height));

            {
                //Onion.Layout.Offset(Onion.Theme.Padding, Onion.Theme.Padding);
                layout.Size(128, Window.Height / 2);
                layout.VerticalLayout();

                gui.Start(75, new ScrollView());

                layout.Size(0, 32);
                layout.FitContainer(1, null);
                layout.Offset(Onion.Theme.Padding, Onion.Theme.Padding);
                Checkbox.Create(ref PasswordCheckbox, "Password text box");

                for (int i = 0; i < 7; i++)
                {
                    layout.Offset(Onion.Theme.Padding, Onion.Theme.Padding);
                    layout.Size(0, 32);
                    layout.FitContainer(1, null);

                    if (Button.Click("Ik besta ook " + i, i))
                        WindowsOpen[i % WindowsOpen.Length] = !WindowsOpen[i % WindowsOpen.Length];
                }

                gui.End();
            }

            {
                const float hotbarHeight = 64;
                layout.FitContainer(1, 0);
                layout.Height(hotbarHeight);
                layout.CenterHorizontal();
                layout.Offset(0, Window.Height - hotbarHeight - Onion.Theme.Padding);

                gui.Start(106, new ScrollView());
                for (int i = 0; i < 4; i++)
                {
                    layout.Size(hotbarHeight - Onion.Theme.Padding * 2, hotbarHeight - Onion.Theme.Padding * 2);
                    layout.CenterVertical();
                    layout.Offset(Onion.Theme.Padding + i * (hotbarHeight - Onion.Theme.Padding), 0);
                    if (ImageButton.Click(Texture.ErrorTexture, ImageContainmentMode.Cover, i))
                    {
                        var a = Resources.Load<Font>("inter-tight.fnt");
                        var b = Resources.Load<Font>("roboto mono.fnt");
                        if (Onion.Theme.Font == b)
                            Onion.Theme.Font = a;
                        else
                            Onion.Theme.Font = b;
                    }
                }
                gui.End();
            }

            {
                layout.Size(128, 24);
                layout.Offset(128, Onion.Theme.Padding);
                Dropdown<string>.Create(DropdownOptions, ref DropdownSelectedIndex);
            }

            {
                layout.Size(150, 24);
                layout.Offset(128 + 128 + Onion.Theme.Padding, Onion.Theme.Padding);
                var s = Scene.GetSystem<OnionSystem>();
                Dropdown<UiDebugOverlay>.CreateForEnum(ref s.DebugOverlay);
            }

            layout.Size(128 + 150, 256);
            layout.Offset(128, 24 + Onion.Theme.Padding);
            TextRect.Create(textRectContents, HorizontalTextAlign.Left, VerticalTextAlign.Top);

            for (int i = 0; i < 2; i++)
            {
                if (!WindowsOpen[i])
                    continue;

                layout.Offset(i * 64, i * 64);
                layout.Size(300, 128);
                DragWindow.Start(textBoxContent, ref WindowsOpen[i], i);
                {
                    layout.FitContainer(1, 1);
                    layout.OffsetSize(0, -24);
                    layout.Offset(0, 24);
                    layout.Offset(Onion.Theme.Padding, Onion.Theme.Padding);
                    gui.Start(1435 + i, new ScrollView());
                    {
                        layout.Size(128, 32);
                        layout.Offset(Onion.Theme.Padding * 2, Onion.Theme.Padding * 2 + 24);
                        Dropdown<string>.Create(DropdownOptions, ref DropdownSelectedIndex, identity: i);

                        layout.Size(128, 32);
                        layout.Offset(Onion.Theme.Padding * 3 + 128, Onion.Theme.Padding * 2 + 24);
                        TextBox.Create(ref textBoxContent, 
                            new TextBoxOptions(placeholder: "Placeholder!", password: i == 0 && PasswordCheckbox), identity: i);
                    }
                    gui.End(); //end container child
                }

                gui.End();
            }

            layout.Size(256, 32);
            layout.Offset(Onion.Theme.Padding, Window.Height / 2 + Onion.Theme.Padding);
            Slider.Float(ref Onion.Theme.Padding, Slider.Direction.Horizontal, new MinMax<float>(0, 8), 0.1f);

            layout.Size(256, 32);
            layout.Offset(Onion.Theme.Padding, Window.Height / 2 + Onion.Theme.Padding * 2 + 32);
            Slider.Float(ref Onion.Theme.Rounding, Slider.Direction.Horizontal, new MinMax<float>(0, 24), 0.1f);

            layout.Size(256, 32);
            layout.Offset(Onion.Theme.Padding, Window.Height / 2 + Onion.Theme.Padding * 3 + 32 * 2);
            Slider.Int(ref Onion.Theme.FontSize, Slider.Direction.Horizontal, new MinMax<int>(8, 24), 1);

            layout.Size(256, 300);
            layout.StickRight();
            layout.StickBottom();
            ColourPicker.Create(ref Onion.Theme.Background.Color);
        }

        public override void Render()
        {

        }
    }
}
