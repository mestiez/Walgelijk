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
                Onion.Layout.Size(128, 32);
                Onion.Layout.Offset(128, Onion.Theme.Padding);
                Dropdown<string>.Create(DropdownOptions, ref DropdownSelectedIndex);
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
        }

        public override void Render()
        {

        }
    }
}
