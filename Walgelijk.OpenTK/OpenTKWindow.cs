using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Diagnostics;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{

    public class OpenTKWindow : Walgelijk.Window
    {
        internal readonly GameWindow window;
        internal readonly OpenTKRenderTarget renderTarget;
        internal readonly OpenTKShaderManager shaderManager;

        private InputHandler inputHandler ;
        private Time time = new Time();
        private Stopwatch stopwatch;

        public OpenTKWindow(string title, Vector2 position, Vector2 size)
        {
            window = new GameWindow((int)size.X, (int)size.Y, GraphicsMode.Default, title, GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.Default);
            renderTarget = new OpenTKRenderTarget();
            renderTarget.Window = this;
            shaderManager = new OpenTKShaderManager();
            inputHandler = new InputHandler(this);
        }

        public override string Title { get => window.Title; set => window.Title = value; }
        public override Vector2 Position { get => new Vector2(window.Location.X, window.Location.Y); set => window.Location = new Point((int)value.X, (int)value.Y); }
        public override int TargetFrameRate { get => (int)window.TargetRenderFrequency; set => window.TargetRenderFrequency = value; }
        public override int TargetUpdateRate { get => (int)window.TargetUpdateFrequency; set => window.TargetUpdateFrequency = value; }
        public override bool VSync { get => window.VSync == VSyncMode.On; set => window.VSync = (value ? VSyncMode.On : VSyncMode.Off); }
        public override bool IsOpen => window.Exists && !window.IsExiting;
        public override bool HasFocus => window.Focused;
        public override Time Time => time;
        public override bool IsVisible { get => window.Visible; set => window.Visible = value; }
        public override bool Resizable { get => window.WindowBorder == WindowBorder.Resizable; set => window.WindowBorder = value ? WindowBorder.Resizable : WindowBorder.Fixed; }
        public override InputState InputState => inputHandler?.InputState ?? default;
        public override RenderTarget RenderTarget => renderTarget;
        public override IShaderManager ShaderManager => shaderManager;

        public override Vector2 Size
        {
            get => new Vector2(window.Width, window.Height);
            set
            {
                window.Width = (int)value.X;
                window.Height = (int)value.Y;
            }
        }

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
            HookIntoEvents();
            window.Run();
        }

        private void HookIntoEvents()
        {
            window.Closing += OnWindowClose;
            window.Resize += OnWindowResize;
            window.Move += OnWindowMove;
            window.FileDrop += OnFireDrop;

            window.UpdateFrame += OnUpdateFrame;
            window.RenderFrame += OnRenderFrame;

            window.Load += OnWindowLoad;
        }

        private void OnWindowLoad(object sender, EventArgs args)
        {
            window.MakeCurrent();
            RenderTarget.Size = Size;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private void OnRenderFrame(object sender, FrameEventArgs obj)
        {
            time.DeltaTime = (float)obj.Time;
            time.SecondsSinceStart = (float)stopwatch.Elapsed.TotalSeconds;

            Game.Scene?.RenderSystems();

            RenderTarget.Clear();
            RenderQueue.RenderAndReset(RenderTarget);
            window.SwapBuffers();
        }

        private void OnUpdateFrame(object sender, FrameEventArgs obj)
        {
            time.DeltaTime = (float)obj.Time;

            Game.Scene?.UpdateSystems();
            inputHandler.Reset();
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
