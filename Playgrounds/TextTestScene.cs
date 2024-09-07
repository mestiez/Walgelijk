using System.Drawing.Drawing2D;
using System.Numerics;
using System.Runtime.InteropServices;
using Walgelijk;
using Walgelijk.Onion;
using Walgelijk.Onion.Assets;
using Walgelijk.Onion.Controls;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

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

        Ui.Animation.DefaultDurationSeconds = 0;
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

        game.UpdateRate = 60;

        return scene;
    }

    public class Typesetter { };

    public class TextTestSystem : Walgelijk.System, IDisposable
    {
        public bool RenderUi = true;

        public enum Gizmos
        {
            GeometryBounds = 0,
            TextBounds = 1,
            Baseline = 2
        }

        public readonly TextMeshGenerator LegacyGenerator = new TextMeshGenerator();
        public readonly Typesetter ModernGenerator = new Typesetter();

        public string Text = "Hallo Wereld!";

        public float LegacyPosition;
        public float ModernPosition;

        public Font Font = Font.Default;
        public int FontSize = 24;
        public float Tracking = 1;
        public float Kerning = 1;

        public HorizontalTextAlign HorizontalAlign = HorizontalTextAlign.Left;
        public VerticalTextAlign VerticalAlign = VerticalTextAlign.Bottom;

        public bool[] gizmoToggles = { false, false, true };

        private const int maxVertexCount = 1024;
        private readonly VertexBuffer vtx = new VertexBuffer(new Vertex[maxVertexCount], new uint[maxVertexCount * 6]);

        public static readonly Color GeometryBoundsCol = Colors.Red;
        public static readonly Color TextBoundsCol = Colors.Blue;
        public static readonly Color BaselineCol = new(0x0acf52);

        private Font[] Fonts =
        [
            Font.Default,
            ..Directory.EnumerateFiles("resources/fonts/", "**.wf").Select(p => Resources.Load<Font>(p, true))
        ];

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
            LegacyGenerator.KerningMultiplier = Kerning;
            LegacyGenerator.HorizontalAlign = HorizontalAlign;
            LegacyGenerator.VerticalAlign = VerticalAlign;
            LegacyGenerator.Color = Ui.Theme.Base.Text.Default;

            //ModernGenerator.Font = Font;
            //ModernGenerator.FontSize = FontSize;
            //ModernGenerator.TrackingMultiplier = Tracking;
            //ModernGenerator.KerningMultiplier = Kerning;
            //ModernGenerator.HorizontalAlign = HorizontalAlign;
            //ModernGenerator.VerticalAlign = VerticalAlign;
            //ModernGenerator.Color = Ui.Theme.Base.Text.Default;

            LegacyPosition = Window.Height * (1 / 3f);
            ModernPosition = Window.Height * (2 / 3f);
        }

        public override void Update()
        {
            if (RenderUi)
            {
                float padding = Ui.Theme.Base.Padding;
                const float ddw = 180;
                float w = Window.Width - padding * 3;
                Ui.Layout.Width(w - ddw).Height(32).Move(padding, padding);
                Ui.StringInputBox(ref Text, default);

                Ui.Layout.Size(ddw, 32).Move(w - ddw + padding * 2, padding);
                Ui.Dropdown(Fonts, ref selectedFontIndex);

                Ui.Layout.Size(Window.Width, 32 + padding * 2).Move(0, padding * 2 + 32).HorizontalLayout();
                Ui.StartScrollView();
                {
                    Ui.Layout.Size(128, 32);
                    Ui.EnumDropdown(ref VerticalAlign);

                    Ui.Layout.Size(128, 32);
                    Ui.EnumDropdown(ref HorizontalAlign);

                    Ui.Layout.Size(64, 32);
                    Ui.Decorators.Tooltip("Font size");
                    Ui.IntStepper(ref FontSize, (8, 144), 1);

                    Ui.Layout.Size(64, 32);
                    Ui.Decorators.Tooltip("Tracking");
                    Ui.FloatStepper(ref Tracking, (0, 4), 0.1f);

                    Ui.Layout.Size(64, 32);
                    Ui.Decorators.Tooltip("Kerning");
                    Ui.FloatStepper(ref Kerning, (0, 20), 0.1f);

                    Ui.Layout.Size(130, 32);
                    Ui.StartGroup(false);
                    {
                        Ui.Layout.Size(130, 14);
                        Ui.Theme.Accent(GeometryBoundsCol);
                        Ui.Checkbox(ref gizmoToggles[0], nameof(Gizmos.GeometryBounds));
                        Ui.Layout.Size(140, 14).Move(0, 18);
                        Ui.Theme.Accent(TextBoundsCol);
                        Ui.Checkbox(ref gizmoToggles[1], nameof(Gizmos.TextBounds));
                    }
                    Ui.End();

                    Ui.Layout.Size(130, 32);
                    Ui.StartGroup(false);
                    {
                        Ui.Layout.Size(130, 14);
                        Ui.Theme.Accent(BaselineCol);
                        Ui.Checkbox(ref gizmoToggles[2], nameof(Gizmos.Baseline));
                    }
                    Ui.End();
                }
                Ui.End();

                if (Input.IsKeyReleased(Key.Tab))
                    RenderUi = false;
            }
            else if (Input.IsKeyReleased(Key.Tab))
                RenderUi = true;

            FontSize = Math.Max(6, FontSize);
            Font = Fonts[selectedFontIndex];
        }

        public override void Render()
        {
            RenderQueue.Add(renderTask);
        }

        private void RenderAction(IGraphics g)
        {
            const float padding = 32;

            var target = g.CurrentTarget;

            var view = target.ViewMatrix;
            var proj = target.ProjectionMatrix;
            target.ProjectionMatrix = target.OrthographicMatrix;
            target.ViewMatrix = Matrix4x4.Identity;
            float ratio = (float)FontSize / Font.Size;

            float x = padding;
            switch (HorizontalAlign)
            {
                case HorizontalTextAlign.Center:
                    x = Window.Width / 2;
                    break;
                case HorizontalTextAlign.Right:
                    x = Window.Width - padding;
                    break;
            }

            Synchronise();

            var legacyPos = new Vector2(x, LegacyPosition);
            var modernPos = new Vector2(x, ModernPosition);

            var r = LegacyGenerator.Generate(Text, vtx.Vertices, vtx.Indices);
            DrawResult(g, ratio, legacyPos, r);

            //r = ModernGenerator.Generate(Text, vtx.Vertices, vtx.Indices);
            //DrawResult(g, ratio, modernPos, r);

            target.ViewMatrix = view;
            target.ProjectionMatrix = proj;
        }

        private void DrawResult(IGraphics g, float ratio, Vector2 pos, TextMeshResult r)
        {
            var target = g.CurrentTarget;
            vtx.ForceUpdate();
            vtx.AmountOfIndicesToRender = r.IndexCount;

            if (gizmoToggles[(int)Gizmos.Baseline])
                DrawLine(g, new Vector2(0, pos.Y), new Vector2(Window.Width, pos.Y), 2, BaselineCol);

            if (gizmoToggles[(int)Gizmos.GeometryBounds])
            {
                var v = r.LocalBounds;
                v.MinX *= ratio;
                v.MinY *= -ratio;
                v.MaxX *= ratio;
                v.MaxY *= -ratio;
                DrawRect(g, v.Translate(pos), 2, GeometryBoundsCol);
            }

            if (gizmoToggles[(int)Gizmos.TextBounds])
            {
                var v = r.LocalTextBounds;
                v.MinX *= ratio;
                v.MinY *= -ratio;
                v.MaxX *= ratio;
                v.MaxY *= -ratio;
                DrawRect(g, v.Translate(pos), 2, TextBoundsCol);
            }

            target.ModelMatrix =
                Matrix4x4.CreateScale(ratio, -ratio, 1) *
                Matrix4x4.CreateTranslation(pos.X, pos.Y, 0);
            g.Draw(vtx, Font.Material);
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

        private void DrawRect(IGraphics g, Rect r, float width, Color color)
        {
            DrawLine(g, r.TopLeft, r.TopRight, width, color);
            DrawLine(g, r.BottomLeft, r.BottomRight, width, color);
            DrawLine(g, r.TopLeft, r.BottomLeft, width, color);
            DrawLine(g, r.BottomRight, r.TopRight, width, color);
        }

        public void Dispose()
        {
            vtx.Dispose();
            lineMat.Dispose();
        }
    }
}
