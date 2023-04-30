using System.Numerics;
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

            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer - 1);
            Draw.Colour = Onion.Theme.Background[ControlState.None].Color;
            Draw.Quad(new Rect(0, 0, Window.Width, Window.Height));

            {
                //Onion.Layout.Offset(Onion.Theme.Padding, Onion.Theme.Padding);
                layout.Size(128, Window.Height / 2);
                layout.VerticalLayout();
                Ui.ScrollView();

                layout.Size(0, 32);
                layout.FitContainer(1, null);
                layout.Move(Onion.Theme.Padding, Onion.Theme.Padding);
                Ui.Checkbox(ref PasswordCheckbox, "Password text box");

                for (int i = 0; i < 7; i++)
                {
                    layout.Move(Onion.Theme.Padding, Onion.Theme.Padding);
                    layout.Size(0, 32);
                    layout.FitContainer(1, null);

                    if (Button.Click("Ik besta ook " + i, i))
                        WindowsOpen[i % WindowsOpen.Length] = !WindowsOpen[i % WindowsOpen.Length];
                }

                Ui.End();
            }

            {
                const float hotbarHeight = 64;
                layout.FitContainer(1, 0);
                layout.Height(hotbarHeight);
                layout.CenterHorizontal();
                layout.Move(0, Window.Height - hotbarHeight - Onion.Theme.Padding);

                Ui.ScrollView();
                for (int i = 0; i < 4; i++)
                {
                    layout.Size(hotbarHeight - Onion.Theme.Padding * 2, hotbarHeight - Onion.Theme.Padding * 2);
                    layout.CenterVertical();
                    layout.Move(Onion.Theme.Padding + i * (hotbarHeight - Onion.Theme.Padding), 0);
                    if (Ui.ClickImageButton(Texture.ErrorTexture, ImageContainmentMode.Cover, i))
                    {
                        var a = Resources.Load<Font>("inter-tight.fnt");
                        var b = Resources.Load<Font>("roboto mono.fnt");
                        if (Onion.Theme.Font == b)
                            Onion.Theme.Font = a;
                        else
                            Onion.Theme.Font = b;
                    }
                }
                Ui.End();
            }

            {
                layout.Size(128, 24);
                layout.Move(128, Onion.Theme.Padding);
                Ui.Dropdown(DropdownOptions, ref DropdownSelectedIndex);
            }

            {
                layout.Size(150, 24);
                layout.Move(128 + 128 + Onion.Theme.Padding, Onion.Theme.Padding);
                var s = Scene.GetSystem<OnionSystem>();
                Ui.EnumDropdown(ref s.DebugOverlay);
            }

            layout.Size(128 + 150, 256);
            layout.Move(128, 24 + Onion.Theme.Padding);
            Ui.TextRect(textRectContents, HorizontalTextAlign.Left, VerticalTextAlign.Top);

            for (int i = 0; i < 2; i++)
            {
                if (!WindowsOpen[i])
                    continue;

                layout.Move(i * 64, i * 64).Size(300, 128).Resizable(new Vector2(146, 115), new Vector2(512));
                Ui.DragWindow(textBoxContent, ref WindowsOpen[i], i);
                {
                    layout.FitContainer(1, 1).Scale(0, -24).Move(0, 24).Move(Onion.Theme.Padding, Onion.Theme.Padding);
                    Ui.ScrollView(i);
                    {
                        layout.Size(128, 32);
                        layout.Move(Onion.Theme.Padding, Onion.Theme.Padding);
                        Ui.Dropdown(DropdownOptions, ref DropdownSelectedIndex, identity: i);

                        layout.Size(256, 32);
                        layout.FitContainer(1, null);
                        layout.Move(Onion.Theme.Padding, Onion.Theme.Padding);
                        layout.StickBottom();
                        Ui.TextBox(ref textBoxContent,
                            new TextBoxOptions(placeholder: "Placeholder!", password: i == 0 && PasswordCheckbox), identity: i);
                    }
                    Ui.End(); //end container child
                }

                Ui.End();
            }

            layout.Size(256, 32);
            layout.Move(Onion.Theme.Padding, Window.Height / 2 + Onion.Theme.Padding);
            Ui.IntSlider(ref Onion.Theme.Padding, Slider.Direction.Horizontal, new MinMax<int>(0, 8), 1);

            layout.Size(256, 32);
            layout.Move(Onion.Theme.Padding, Window.Height / 2 + Onion.Theme.Padding * 2 + 32);
            Ui.FloatSlider(ref Onion.Theme.Rounding, Slider.Direction.Horizontal, new MinMax<float>(0, 24), 0.1f);

            layout.Size(256, 32);
            layout.Move(Onion.Theme.Padding, Window.Height / 2 + Onion.Theme.Padding * 3 + 32 * 2);
            Ui.FloatSlider(ref Onion.Theme.FocusBoxSize, Slider.Direction.Horizontal, new MinMax<float>(0, 24), 1);

            {
                layout.Size(32, (Window.Height / 2 + Onion.Theme.Padding * 3 + 32 * 2 + 32) - (Window.Height / 2 + Onion.Theme.Padding));
                layout.Move(Onion.Theme.Padding * 2 + 256, Window.Height / 2 + Onion.Theme.Padding);
                Ui.FloatSlider(ref Onion.Theme.Accent.Default.R, Slider.Direction.Vertical, new MinMax<float>(0, 1), 0.01f);

                layout.Size(32, (Window.Height / 2 + Onion.Theme.Padding * 3 + 32 * 2 + 32) - (Window.Height / 2 + Onion.Theme.Padding));
                layout.Move(Onion.Theme.Padding * 2 + 256, Window.Height / 2 + Onion.Theme.Padding);
                layout.Move(32 + Onion.Theme.Padding, 0);
                Ui.FloatSlider(ref Onion.Theme.Accent.Default.B, Slider.Direction.Vertical, new MinMax<float>(0, 1), 0.01f);

                layout.Size(32, (Window.Height / 2 + Onion.Theme.Padding * 3 + 32 * 2 + 32) - (Window.Height / 2 + Onion.Theme.Padding));
                layout.Move(Onion.Theme.Padding * 2 + 256, Window.Height / 2 + Onion.Theme.Padding);
                layout.Move(32 + Onion.Theme.Padding, 0);
                layout.Move(32 + Onion.Theme.Padding, 0);
                Ui.FloatSlider(ref Onion.Theme.Accent.Default.G, Slider.Direction.Vertical, new MinMax<float>(0, 1), 0.01f);

                layout.Size(256 + ((32 + Onion.Theme.Padding) * 3), 32);
                layout.Move(Onion.Theme.Padding, Window.Height / 2 + Onion.Theme.Padding * 4 + 32 * 3);
                Ui.FloatSlider(ref Onion.Animation.DefaultDurationSeconds, Slider.Direction.Horizontal, new MinMax<float>(float.Epsilon, 1), 0.01f);
            }

            layout.Size(256, 300).StickRight().StickBottom();
            Ui.ColourPicker(ref Onion.Theme.Background.Default.Color);
        }

        public override void Render()
        {

        }
    }
}
