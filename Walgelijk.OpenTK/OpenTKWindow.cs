using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{
    public class OpenTKWindow : Walgelijk.Window
    {
        internal readonly GameWindow window;
        internal readonly OpenTKRenderTarget renderTarget;
        internal readonly OpenTKShaderManager shaderManager;

        public OpenTKWindow(string title, Vector2 position, Vector2 size)
        {
            window = new GameWindow((int)size.X, (int)size.Y, GraphicsMode.Default, title);
            renderTarget = new OpenTKRenderTarget();
            renderTarget.Window = this;
            shaderManager = new OpenTKShaderManager();
        }

        public override string Title { get => window.Title; set => window.Title = value; }
        public override Vector2 Position { get => new Vector2(window.Location.X, window.Location.Y); set => window.Location = new Point((int)value.X, (int)value.Y); }
        public override Vector2 Size
        {
            get => new Vector2(window.Width, window.Height);
            set
            {
                window.Width = (int)value.X;
                window.Height = (int)value.Y;
            }
        }
        public override int TargetFrameRate { get => (int)window.TargetRenderFrequency; set => window.TargetRenderFrequency = value; }
        public override int TargetUpdateRate { get => (int)window.TargetUpdateFrequency; set => window.TargetUpdateFrequency = value; }

        public override bool IsOpen => window.Exists && !window.IsExiting;

        public override bool HasFocus => window.Focused;

        public override bool IsVisible { get => window.Visible; set => window.Visible = value; }
        public override bool Resizable { get => window.WindowBorder == WindowBorder.Resizable; set => window.WindowBorder = value ? WindowBorder.Resizable : WindowBorder.Fixed; }

        public override InputState InputState => default;

        public override RenderTarget RenderTarget => renderTarget;

        public override IShaderManager ShaderManager => shaderManager;

        public override void Close()
        {
            window.Close();
        }

        public override Vector2 ScreenToWindowPoint(Vector2 point)
        {
            var pos = window.PointToClient(new Point((int)point.X, (int)point.Y));
            return new Vector2(pos.X, pos.Y);
        }

        public override void StartLoop()
        {
            window.Closing += OnWindowClose;
            window.Resize += OnWindowResize;
            window.Move += OnWindowMove;
            window.FileDrop += OnFireDrop;

            window.UpdateFrame += OnUpdateFrame;
            window.RenderFrame += OnRenderFrame;

            window.Load += OnWindowLoad;

            window.VSync = VSyncMode.Off;

            window.Run();
        }

        private void OnWindowLoad(object sender, EventArgs args)
        {
            window.MakeCurrent();
            RenderTarget.Size = Size;
        }

        private void OnRenderFrame(object sender, FrameEventArgs obj)
        {
            RenderTarget.Clear();

            RenderQueue.RenderAndReset(RenderTarget);

            window.SwapBuffers();

            Console.WriteLine(Math.Round(obj.Time * 1000, 2) + "ms");
        }

        private void OnUpdateFrame(object sender, FrameEventArgs obj)
        {
            Game.Scene?.ExecuteSystems();
        }

        private void OnFireDrop(object sender, FileDropEventArgs obj)
        {
            InvokeFileDropEvent(new[] { obj.FileName });
        }

        private void OnWindowMove(object sender, EventArgs args)
        {
            InvokeMoveEvent(Position);
        }

        private void OnWindowResize(object sender, EventArgs args)
        {
            RenderTarget.Size = new Vector2(window.Width, window.Height);
            InvokeResizeEvent(Size);
        }

        private void OnWindowClose(object sender, global::System.ComponentModel.CancelEventArgs obj)
        {
            InvokeCloseEvent();
        }

        public override Vector2 WindowToScreenPoint(Vector2 point)
        {
            var pos = window.PointToScreen(new Point((int)point.X, (int)point.Y));
            return new Vector2(pos.X, pos.Y);
        }
    }
}
