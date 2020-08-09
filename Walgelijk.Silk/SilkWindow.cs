using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.Windowing.Common;
using System;
using System.Numerics;
using System.Threading;

namespace Walgelijk.SilkImplementation
{
    public class SilkRenderTarget : Walgelijk.RenderTarget
    {
        private Vector2 size;

        public override Vector2 Size { get => size; set => size = value; }
        public override Color ClearColour { get; set; }

        public override void Draw(VertexBuffer vertexBuffer, Material material)
        {
            
        }
    }

    public class SilkWindow : Walgelijk.Window
    {
        private IWindow window;
        private bool hasFocus;
        private IInputContext input;
        private InputState inputState;
        private RenderTarget renderTarget;

        public SilkWindow(string title, Vector2 position, Vector2 size) : base(title, position, size)
        {
            window = Silk.NET.Windowing.Window.Create(new WindowOptions
            {
                Size = new System.Drawing.Size((int)size.X, (int)size.Y),
                Title = title,
                Position = new System.Drawing.Point((int)position.X, (int)position.Y)
            });

            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.FocusChanged += OnFocusChanged;
        }

        public override string Title { get => window.Title; set => window.Title = value; }

        public override Vector2 Position { get => new Vector2(window.Position.X, window.Position.Y); set => window.Position = new System.Drawing.Point((int)value.X, (int)value.Y); }

        public override Vector2 Size { get => new Vector2(window.Size.Width, window.Size.Height); set => window.Size = new System.Drawing.Size((int)value.X, (int)value.Y); }

        public override bool IsVisible { get => window.IsVisible; set => window.IsVisible = value; }

        public override bool Resizable { get => true; set => throw new NotImplementedException(); }

        public override bool IsOpen => !window.IsClosing;

        public override bool HasFocus => hasFocus;

        public override RenderTarget RenderTarget => renderTarget;

        public override InputState InputState => inputState;

        public override int TargetFrameRate { get => (int)window.FramesPerSecond; set => window.FramesPerSecond = value; }

        public override int TargetUpdateRate { get => (int)window.UpdatesPerSecond; set => window.UpdatesPerSecond = value; }

        public override void StartLoop()
        {
            window.Run();
        }

        private void OnRender(double dt)
        {
            
        }

        private void OnUpdate(double dt)
        {
        }

        private void OnLoad()
        {
            window.Resize += (s) =>
            {
                renderTarget.Size = Size;
                InvokeResizeEvent(new Vector2(s.Width, s.Height));
            };
            window.Move += (p) => InvokeMoveEvent(new Vector2(p.X, p.Y));
            window.FileDrop += (p) => InvokeFileDropEvent(p);
            window.Closing += () => InvokeCloseEvent();

            renderTarget = new SilkRenderTarget();
            renderTarget.Size = Size;

            input = window.CreateInput();
        }

        private void OnFocusChanged(bool hasFocus)
        {
            this.hasFocus = hasFocus;
        }

        public override void Close()
        {
            window.Close();
        }

        public override Vector2 ScreenToWindowPoint(Vector2 screen)
        {
            var p = window.PointToClient(new System.Drawing.Point((int)screen.X, (int)screen.Y));
            return new Vector2(p.X, p.Y);
        }

        public override Vector2 WindowToScreenPoint(Vector2 screen)
        {
            var p = window.PointToScreen(new System.Drawing.Point((int)screen.X, (int)screen.Y));
            return new Vector2(p.X, p.Y);
        }
    }
}
