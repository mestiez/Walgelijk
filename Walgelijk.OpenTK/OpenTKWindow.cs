using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{
    public class OpenTKWindow : Walgelijk.Window
    {
        private readonly GameWindow window;
        private readonly OpenTKRenderTarget renderTarget;

        public OpenTKWindow(string title, Vector2 position, Vector2 size)
        {
            window = new GameWindow((int)size.X, (int)size.Y, GraphicsMode.Default, title);
            //new GameWindowSettings
            //{
            //    RenderFrequency = 60,
            //    UpdateFrequency = 60
            //},
            //new NativeWindowSettings
            //{
            //    Title = title,
            //    Location = new OpenToolkit.Mathematics.Vector2i((int)position.X, (int)position.Y),
            //    Size = new OpenToolkit.Mathematics.Vector2i((int)size.X, (int)size.Y),
            //});

            renderTarget = new OpenTKRenderTarget();
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

            window.Run();
        }

        private void OnWindowLoad(object sender, EventArgs args)
        {
            window.MakeCurrent();
            RenderTarget.Size = Size;
        }
        VertexBuffer buffer = new VertexBuffer(new[] {
            new Vertex(0,0),
            new Vertex(1,0),
            new Vertex(0,1),
            });
        private void OnRenderFrame(object sender, FrameEventArgs obj)
        {
            RenderTarget.Clear();

            RenderTarget.Draw(buffer, null);

            window.SwapBuffers();
        }

        private void OnUpdateFrame(object sender, FrameEventArgs obj)
        {

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

        private void OnWindowClose(object sender, System.ComponentModel.CancelEventArgs obj)
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
