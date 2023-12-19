using System.Data;
using System.Numerics;
using Walgelijk;
using Walgelijk.Imgui;
using Walgelijk.Onion;
using Walgelijk.Onion.Controls;
using Walgelijk.Onion.Decorators;
using Walgelijk.SimpleDrawing;
using Button = Walgelijk.Onion.Controls.Button;

namespace Playgrounds;

public struct IMGUIScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        Gui.SetCursorStack = false;
        //game.Compositor.Flags = RenderTextureFlags.HDR;
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

        public static Color PickedColour = Colors.Purple;
        private static bool EditableAlpha = true;
        public static int DropdownSelectedIndex;
        private static bool PasswordCheckbox = true;

        public readonly static bool[] WindowsOpen = { true, true };

        private string textRectContents =
@"In life there are those who may seem
Undeserving of a smile or a beam
Of kindness, warmth, and empathy too
Yet still we must show them love anew

For who are we to judge and decide
Who deserves our love and who should be denied?
We must rise above our narrow view
And recognize their struggles too

Perhaps behind their hardened shell
Lies a heart that's been through hell
Or maybe they're just having a bad day
And a little kindness goes a long way

So let us not withhold our grace
Or turn our backs on those who need a loving embrace
For every single human being
Deserves kindness and love, a smile on their face";

        private string textBoxContent = "This is the only reliable way to contact us.";
        private int buttonCount = 20;
        private bool onlyEverySecond = false;

        public int SelectedDemo = 0;

        public override void Update()
        {
            if (Input.IsKeyReleased(Key.D1))
                SelectedDemo = 0;
            if (Input.IsKeyReleased(Key.D2))
                SelectedDemo = 1; 
            if (Input.IsKeyReleased(Key.D3))
                SelectedDemo = 2;

            switch (SelectedDemo)
            {
                case 0:
                    MainDemo();
                    break;
                case 1:
                    ManyScrolling();
                    break;
                case 2:
                    AutoSizeTest();
                    break;
            }
        }

        private void AutoSizeTest()
        {
            Ui.Layout.FitContent();
            Ui.StartDragWindow("Huh?");
            {
                Ui.Layout.Size(300, 32).StickTop().StickLeft();
                Ui.Button("Button A");      
                
                Ui.Layout.Size(300, 32).StickTop().StickLeft().Move(0,32);
                Ui.Button("Button B");

                Ui.Layout.PreferredSize().Width(300).Move(0,64);
                Ui.TextRect(textRectContents, HorizontalTextAlign.Left, VerticalTextAlign.Top);
            }
            Ui.End();
        }
        
        private void ManyScrolling()
        {
            Ui.Layout.Width(300).FitHeight(false).CenterHorizontal().VerticalLayout();
            Ui.StartScrollView(true);
            {
                for (int i = 0; i < buttonCount; i++)
                {
                    if (!onlyEverySecond || i % 2 != 0)
                    {
                        Ui.Layout.FitWidth().CenterVertical().Height(32).StickTop().StickLeft();
                        Ui.Theme.Foreground((Appearance)Color.FromHsv((i / 10f) % 1, 0.5f, 0.65f)).Once();
                        var r = Ui.Button("Oma Hondje #" + i, identity: i);
                        if (r)
                            Logger.Debug(i.ToString());
                        Ui.Label("dit is een label", identity: i);
                    }
                }
            }
            Ui.End();
            Ui.Layout.Size(200, 32).StickLeft().StickTop();
            Ui.IntSlider(ref buttonCount, Direction.Horizontal, (0, 30), 1, "{0} buttons");
            Ui.Layout.Size(200, 32).StickRight().StickTop();
            Ui.Checkbox(ref onlyEverySecond, nameof(onlyEverySecond));
        }

        private void MainDemo()
        {
            var layout = Onion.Layout;

            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Order = new RenderOrder(Onion.Configuration.RenderLayer - 1);
            Draw.Colour = Onion.Theme.Base.Background[ControlState.Active].Color.Brightness(1.2f);
            Draw.Quad(new Rect(0, 0, Window.Width, Window.Height));

            {
                //Onion.Layout.Offset(Onion.Theme.Padding, Onion.Theme.Padding);
                layout.Size(128, Window.Height / 2);
                layout.VerticalLayout();
                Ui.StartScrollView();

                layout.Size(0, 32);
                layout.FitContainer(1, null);
                layout.Move(Onion.Theme.Base.Padding, Onion.Theme.Base.Padding);
                Ui.Checkbox(ref PasswordCheckbox, "Password text box");

                for (int i = 0; i < 7; i++)
                {
                    layout.Move(Onion.Theme.Base.Padding, Onion.Theme.Base.Padding);
                    layout.Size(0, 32);
                    layout.FitContainer(1, null);
                     
                    if (Button.Click("Ik besta ook " + i, i))
                        WindowsOpen[i % WindowsOpen.Length] = !WindowsOpen[i % WindowsOpen.Length];
                }

                layout.Move(Onion.Theme.Base.Padding).Height(32).FitWidth();
                var type = Window.WindowType;
                if (Ui.EnumDropdown(ref type))
                    Window.WindowType = type;

                Ui.End();
            }

            {
                const float hotbarHeight = 64;
                layout.FitContainer(1, 0).Height(hotbarHeight).CenterHorizontal().StickBottom();
                //layout.Move(0, Window.Height - hotbarHeight - Onion.Theme.Base.Padding);

                Ui.StartScrollView();
                for (int i = 0; i < 4; i++)
                {
                    layout.Size(hotbarHeight - Onion.Theme.Base.Padding * 2, hotbarHeight - Onion.Theme.Base.Padding * 2);
                    layout.CenterVertical();
                    layout.Move(Onion.Theme.Base.Padding + i * (hotbarHeight - Onion.Theme.Base.Padding), 0);
                    if (Ui.ClickImageButton(Texture.ErrorTexture, ImageContainmentMode.Cover, i))
                    {
                        var a = Resources.Load<Font>("RobotoMono-Regular.wf");
                        var b = Resources.Load<Font>("roboto mono.fnt");
                        if (Onion.Theme.Base.Font == b)
                            Onion.Theme.Base.Font = a;
                        else
                            Onion.Theme.Base.Font = b;
                    }
                }

                Ui.End();
            }

            layout.Size(128, 256).StickRight().StickTop().VerticalLayout();
            Ui.StartDummy();
            {
                layout.Size(128, 32);
                Ui.Decorators.Tooltip("Animation duration");
                Ui.FloatInputBox(ref Game.Console.UI.AnimationDuration, (0, 1));

                layout.Size(128, 32);
                Ui.Decorators.Tooltip("Game.Console.UI.FilterWidth");
                Ui.IntInputBox(ref Game.Console.UI.FilterWidth, (0, 256));
                layout.Size(128, 32);
                Ui.Decorators.Tooltip("Padding");
                Ui.IntInputBox(ref Onion.Theme.Base.Padding, (-4, 16));

                Ui.Spacer(16);
                Ui.Label("Font size");
                layout.Size(128, 32);
                Ui.Decorators.Tooltip("Font size");
                Ui.IntSlider(ref Onion.Theme.Base.FontSize.Default, Direction.Horizontal, (8, 24), 1, "{0}px");

                Ui.Spacer(16);
                layout.Size(128, 32);
                Ui.Checkbox(ref EditableAlpha, "Editable alpha");
            }
            Ui.End();

            {
                layout.Size(128, 24);
                layout.Move(128, Onion.Theme.Base.Padding);
                if (Ui.Dropdown(DropdownOptions, ref DropdownSelectedIndex) && DropdownSelectedIndex == 1)
                    Logger.Log("Bell sound! \a");
            }

            {
                layout.Size(150, 24);
                layout.Move(128 + 128 + Onion.Theme.Base.Padding, Onion.Theme.Base.Padding);
                var s = Scene.GetSystem<OnionSystem>();
                if (Ui.EnumDropdown(ref s.DebugOverlay))
                    Logger.Log("Debug overlay chosen!");
            }

            {
                layout.Size(512, Window.Height / 2 - (24 + Onion.Theme.Base.Padding)).Move(128, 24 + Onion.Theme.Base.Padding);
                layout.VerticalLayout();
                Ui.StartScrollView();
                {
                    layout.PreferredSize().FitWidth();
                    Ui.TextRect(textRectContents, HorizontalTextAlign.Left, VerticalTextAlign.Top);
                }
                Ui.End();
            }

            for (int i = 0; i < 2; i++)
            {
                if (!WindowsOpen[i])
                    continue;

                layout.Move(i * 64, i * 64).Size(300, 256).Resizable(new Vector2(148), new Vector2(512));
                Ui.Decorators.Tooltip("DPI is " + Window.DPI);
                Ui.Theme.Text(Colors.Yellow);
                Ui.StartDragWindow(textBoxContent, ref WindowsOpen[i], i);
                {
                    layout.FitContainer().StickTop().StickLeft().Scale(0, Onion.Theme.Base.Padding);
                    //Ui.Theme.Padding(0).Once();
                    Ui.StartScrollView(false, i);
                    {
                        layout.Size(128, 32);
                        Ui.Decorators.Add(new HoverCrosshairDecorator());
                        Ui.Decorators.Tooltip("This control has a HoverCrosshairDecorator that adds a cool Nurose-like outline on hover");
                        if (Ui.Dropdown(DropdownOptions, ref DropdownSelectedIndex, identity: i))
                            Logger.Log("Window dropdown selected!");

                        layout.FitContainer(1, 1, false).Move(0, 32).Scale(0, -80);
                        Ui.StartScrollView(false, i);
                        {
                            layout.Height(32).FitWidth().Move(Onion.Theme.Base.Padding, Onion.Theme.Base.Padding);
                            Ui.Decorators.Tooltip("Onion.Configuration.SoundVolume");
                            Ui.FloatSlider(ref Onion.Configuration.SoundVolume, Direction.Horizontal, (0, 1), identity: i);

                            Ui.Theme.Text(Colors.Red).Once();
                            layout.Size(256, 590).FitWidth().Move(Onion.Theme.Base.Padding, Onion.Theme.Base.Padding).Move(0, 32).PreferredSize();
                            Ui.TextRect(textRectContents, HorizontalTextAlign.Left, VerticalTextAlign.Top, identity: i);
                        }
                        Ui.End();

                        layout.Size(32, 32);
                        layout.FitContainer(1, null, false);
                        layout.StickBottom();
                        Ui.StringInputBox(ref textBoxContent, new TextBoxOptions(placeholder: "Placeholder!", password: i == 0 && PasswordCheckbox), identity: i);
                    }
                    Ui.End(); //end container child
                }
                Ui.Theme.Pop();

                Ui.End();
            }

            layout.Size(256, 32);
            layout.Move(Onion.Theme.Base.Padding, Window.Height / 2 + Onion.Theme.Base.Padding);
            Ui.IntSlider(ref Onion.Theme.Base.Padding, Direction.Horizontal, new MinMax<int>(0, 8), 1, "Padding: {0}px");

            layout.Size(256, 32);
            layout.Move(Onion.Theme.Base.Padding, Window.Height / 2 + Onion.Theme.Base.Padding * 2 + 32);
            Ui.FloatSlider(ref Onion.Theme.Base.Rounding, Direction.Horizontal, new MinMax<float>(0, 24), 0.5f, "Rounding: {0}px");

            layout.Size(256, 32);
            layout.Move(Onion.Theme.Base.Padding, Window.Height / 2 + Onion.Theme.Base.Padding * 3 + 32 * 2);
            Ui.FloatSlider(ref Onion.Theme.Base.FocusBoxSize, Direction.Horizontal, new MinMax<float>(0, 24), 1);

            {
                layout.Size(32, (Window.Height / 2 + Onion.Theme.Base.Padding * 3 + 32 * 2 + 32) - (Window.Height / 2 + Onion.Theme.Base.Padding));
                layout.Move(Onion.Theme.Base.Padding * 2 + 256, Window.Height / 2 + Onion.Theme.Base.Padding);
                Ui.FloatSlider(ref Onion.Theme.Base.Accent.Default.R, Direction.Vertical, new MinMax<float>(0, 1), 0.01f, "R");

                layout.Size(32, (Window.Height / 2 + Onion.Theme.Base.Padding * 3 + 32 * 2 + 32) - (Window.Height / 2 + Onion.Theme.Base.Padding));
                layout.Move(Onion.Theme.Base.Padding * 2 + 256, Window.Height / 2 + Onion.Theme.Base.Padding);
                layout.Move(32 + Onion.Theme.Base.Padding, 0);
                Ui.FloatSlider(ref Onion.Theme.Base.Accent.Default.B, Direction.Vertical, new MinMax<float>(0, 1), 0.01f, "G");

                layout.Size(32, (Window.Height / 2 + Onion.Theme.Base.Padding * 3 + 32 * 2 + 32) - (Window.Height / 2 + Onion.Theme.Base.Padding));
                layout.Move(Onion.Theme.Base.Padding * 2 + 256, Window.Height / 2 + Onion.Theme.Base.Padding);
                layout.Move(32 + Onion.Theme.Base.Padding, 0);
                layout.Move(32 + Onion.Theme.Base.Padding, 0);
                Ui.FloatSlider(ref Onion.Theme.Base.Accent.Default.G, Direction.Vertical, new MinMax<float>(0, 1), 0.01f, "B");

                layout.Size(256 + ((32 + Onion.Theme.Base.Padding) * 3), 32);
                layout.Move(Onion.Theme.Base.Padding, Window.Height / 2 + Onion.Theme.Base.Padding * 4 + 32 * 3);
                Ui.FloatSlider(ref Onion.Animation.DefaultDurationSeconds, Direction.Horizontal, new MinMax<float>(float.Epsilon, 1), 0.01f);
            }

            layout.Size(290, 320).StickRight().StickBottom().Resizable(new Vector2(148), new Vector2(1024));
            Ui.StartDragWindow(nameof(Ui.ColourPicker));
            {
                layout.FitContainer().StickTop().StickLeft();
                Ui.ColourPicker(ref PickedColour, EditableAlpha);
            }
            Ui.End();
        }

        public override void Render()
        {

        }
    }
}


public readonly struct HoverCrosshairDecorator : IDecorator
{
    public void RenderBefore(in ControlParams p)
    {
    }

    public void RenderAfter(in ControlParams p)
    {
        float pr = p.Instance.GetTransitionProgress();
        if (!p.Instance.IsHover)
            pr = 1 - pr;
        if (pr <= float.Epsilon)
            return;
        pr = Easings.Quad.Out(pr);

        float t = p.GameState.Time.SecondsSinceSceneChangeUnscaled - p.Instance.LastStateChangeTime;

        Draw.ResetDrawBounds();
        Draw.ResetTexture();
        Draw.Order = Draw.Order.OffsetLayer(1);
        Draw.Colour = Colors.Transparent;
        Draw.OutlineColour = Onion.Theme.Base.Accent[ControlState.None];
        Draw.OutlineColour.A = pr;
        Draw.OutlineWidth = 8;
        Draw.Quad(p.Instance.Rects.ComputedGlobal.Expand(3 + pr * pr * Utilities.MapRange(-1, 1, 1, 3, MathF.Sin(t * 5))));
        Draw.OutlineWidth = 0;
    }
}