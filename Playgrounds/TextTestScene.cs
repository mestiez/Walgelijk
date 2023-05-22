using System.Drawing.Drawing2D;
using System.Numerics;
using System.Runtime.InteropServices;
using Walgelijk;
using Walgelijk.Onion;
using Walgelijk.Onion.Assets;
using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public readonly struct TextTestScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new OnionSystem());
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new TextTestSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = Colors.WhiteSmoke
        });

        Ui.Theme.Base.Rounding = 0;
        Ui.Theme.Base.Text = Colors.Black;
        Ui.Theme.Base.Accent = Colors.Sky;
        Ui.Theme.Base.OutlineColour = Colors.Black;
        Ui.Theme.Base.OutlineWidth = new StateDependent<int>(1, 2, 1, 2);
        Ui.Theme.Base.FocusBoxColour = Colors.Black.WithAlpha(0.5f);
        Ui.Theme.Base.FocusBoxWidth = 1;
        Ui.Theme.Base.FocusBoxSize = 2;
        Ui.Theme.Base.Background = (Appearance)Colors.White;
        Ui.Theme.Base.Image = Colors.Black;
        Ui.Theme.Base.Foreground = new StateDependent<Appearance>(Colors.WhiteSmoke, Colors.White, Colors.WhiteSmoke);

        Draw.CacheTextMeshes = -1;

        return scene;
    }

    public class TextTestSystem : Walgelijk.System, IDisposable
    {
        public enum Gizmos
        {
            GeometryBounds = 0,
            TextBounds = 1
        }

        public readonly TextMeshGenerator LegacyGenerator = new TextMeshGenerator();
        public readonly Typesetter ModernGenerator = new Typesetter();

        public string Text = "Hallo Wereld!";

        public Vector2 LegacyPosition;
        public Vector2 ModernPosition;

        public Font Font = Font.Default;
        public int FontSize = 24;
        public float Tracking = 1;

        public HorizontalTextAlign HorizontalAlign = HorizontalTextAlign.Left;
        public VerticalTextAlign VerticalAlign = VerticalTextAlign.Bottom;

        public bool[] gizmoToggles = { false, false };

        private const int maxVertexCount = 1024;
        private readonly VertexBuffer vtx = new VertexBuffer(new Vertex[maxVertexCount], new uint[maxVertexCount * 6]);

        private Font[] Fonts =
        {
            Font.Default,
            Resources.Load<Font>("arial narrow bold.fnt"),
            Resources.Load<Font>("broadway.fnt"),
            Resources.Load<Font>("cambria.fnt"),
            Resources.Load<Font>("inter.fnt"),
            Resources.Load<Font>("inter-tight.fnt"),
            Resources.Load<Font>("roboto mono.fnt"),
        };

        private int selectedFontIndex = 0;
        private ActionRenderTask renderTask;
        private Material lineMat = DrawingMaterialCreator.Create(Texture.White);

        public TextTestSystem()
        {
            renderTask = new ActionRenderTask(RenderAction);
        }

        public void Synchronise()
        {
            lineMat.SetUniform(DrawingMaterialCreator.TintUniform, Colors.Red);
            lineMat.SetUniform(DrawingMaterialCreator.ScaleUniform, Vector2.One);

            LegacyGenerator.Font = Font;
            LegacyGenerator.TrackingMultiplier = Tracking;
            LegacyGenerator.HorizontalAlign = HorizontalAlign;
            LegacyGenerator.VerticalAlign = VerticalAlign;
            LegacyGenerator.Color = Ui.Theme.Base.Text.Default;

            ModernGenerator.Font = Font;
            ModernGenerator.FontSize = FontSize;
            ModernGenerator.TrackingMultiplier = Tracking;
            ModernGenerator.HorizontalAlign = HorizontalAlign;
            ModernGenerator.VerticalAlign = VerticalAlign;
            ModernGenerator.Color = Ui.Theme.Base.Text.Default;

            LegacyPosition = new Vector2(32, Window.Height * (1 / 3f));
            ModernPosition = new Vector2(32, Window.Height * (2 / 3f));
        }

        public override void Update()
        {
            float padding = Ui.Theme.Base.Padding;
            const float ddw = 180;
            float w = Window.Width - padding * 3;
            Ui.Layout.Width(w - ddw).Height(32).Move(padding, padding);
            Ui.StringInputBox(ref Text, default);

            Ui.Layout.Size(ddw, 32).Move(w - ddw + padding * 2, padding);
            Ui.Dropdown(Fonts, ref selectedFontIndex);

            Ui.Layout.Size(128, 32).Move(padding, padding * 2 + 32);
            Ui.EnumDropdown(ref VerticalAlign);

            Ui.Layout.Size(128, 32).Move(padding * 2 + 128, padding * 2 + 32);
            Ui.EnumDropdown(ref HorizontalAlign);

            Ui.Layout.Size(133, 32).Move(padding * 3 + 128 * 2, padding * 2 + 32);
            Ui.Decorators.Tooltip("Font size");
            Ui.IntStepper(ref FontSize, (8, 72), 1);

            Ui.Layout.Size(100, 32).Move(padding * 5 + 128 * 3, padding * 2 + 32);
            Ui.Decorators.Tooltip("Tracking");
            Ui.FloatSlider(ref Tracking, Direction.Horizontal, (0, 4), 0.1f, "{0}x");

            FontSize = Math.Max(6, FontSize);
            Font = Fonts[selectedFontIndex];
        }

        public override void Render()
        {
            RenderQueue.Add(renderTask);
        }

        private void RenderAction(IGraphics g)
        {
            const float s = 1;

            var target = g.CurrentTarget;

            var view = target.ViewMatrix;
            var proj = target.ProjectionMatrix;
            target.ProjectionMatrix = target.OrthographicMatrix;
            target.ViewMatrix = Matrix4x4.Identity;
            float ratio = (float)FontSize / Font.Size;

            Synchronise();

            var r = PrepareLegacy();
            var a = LegacyPosition;
            var b = new Vector2(LegacyPosition.X + r.LocalBounds.Width * ratio, LegacyPosition.Y);
            DrawLine(g, a, b, 2, Colors.Red);
            DrawLine(g, new Vector2(a.X, a.Y - r.LocalBounds.Height * ratio), new Vector2(b.X, b.Y - r.LocalBounds.Height * ratio), 2, Colors.Blue);

            target.ModelMatrix =
                Matrix4x4.CreateScale(s * ratio, -s * ratio, 1) *
                Matrix4x4.CreateTranslation(LegacyPosition.X, LegacyPosition.Y, 0);
            g.Draw(vtx, Font.Material);

            r = PrepareModern();
            target.ModelMatrix =
                Matrix4x4.CreateScale(s, -s, 1) *
                Matrix4x4.CreateTranslation(ModernPosition.X, ModernPosition.Y, 0);
            g.Draw(vtx, Font.Material);

            target.ViewMatrix = view;
            target.ProjectionMatrix = proj;
        }

        private void DrawLine(IGraphics g, Vector2 from, Vector2 to, float width, Color color)
        {
            var delta = to - from;
            var distance = delta.Length();
            var dir = delta / distance;

            float deg = MathF.Atan2(dir.Y, dir.X);

            g.CurrentTarget.ModelMatrix =
                Matrix4x4.CreateScale(distance, width, 1) *
                Matrix4x4.CreateRotationZ(deg) *
                Matrix4x4.CreateTranslation(from.X, from.Y, 0);
            lineMat.SetUniform("tint", color);
            g.Draw(PrimitiveMeshes.Quad, lineMat);
        }

        private TextMeshResult PrepareModern()
        {
            var r = ModernGenerator.Generate(Text, vtx.Vertices, vtx.Indices);
            vtx.ForceUpdate();
            vtx.AmountOfIndicesToRender = r.IndexCount;
            return r;
        }

        private TextMeshResult PrepareLegacy()
        {
            var r = LegacyGenerator.Generate(Text, vtx.Vertices, vtx.Indices);
            vtx.ForceUpdate();
            vtx.AmountOfIndicesToRender = r.IndexCount;
            return r;
        }

        public void Dispose()
        {
            vtx.Dispose();
        }
    }
}