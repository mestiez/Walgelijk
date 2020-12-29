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
    /// <summary>
    /// OpenTK implementation of <see cref="Window"/>
    /// </summary>
    public class OpenTKWindow : Window
    {
        internal readonly GameWindow window;
        internal readonly OpenTKWindowRenderTarget renderTarget;

        private readonly InputHandler inputHandler;
        internal readonly OpenTKGraphics internalGraphics;

        private Time time = new Time();
        private Stopwatch stopwatch;

        public OpenTKWindow(string title, Vector2 position, Vector2 size)
        {
            window = new GameWindow((int)size.X, (int)size.Y, GraphicsMode.Default, title, GameWindowFlags.Default, DisplayDevice.Default, 0, 0, GraphicsContextFlags.Default);

            if (position.X >= 0 && position.Y >= 0)
                Position = position;

            renderTarget = new OpenTKWindowRenderTarget();
            renderTarget.Window = this;

            inputHandler = new InputHandler(this);
            internalGraphics = new OpenTKGraphics();
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
        public override IGraphics Graphics => internalGraphics;

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

        public override Vector2 WindowToScreenPoint(Vector2 point)
        {
            var pos = window.PointToScreen(new Point((int)point.X, (int)point.Y));
            return new Vector2(pos.X, pos.Y);
        }

        //TODO dit is eigenlijk iets van de rendertarget
        public override Vector2 WorldToWindowPoint(Vector2 point)
        {
            var result = Vector2.Transform(new Vector2(point.X, point.Y), renderTarget.ProjectionMatrix * renderTarget.ViewMatrix);
            result.X *= renderTarget.Size.X;
            result.Y *= renderTarget.Size.Y;
            return result;
        }

        //TODO dit is eigenlijk iets van de rendertarget
        public override Vector2 WindowToWorldPoint(Vector2 point)
        {
            point /= renderTarget.Size;
            point.X -= 0.5f;
            point.Y += 0.5f;
            point.Y = 1 - point.Y;
            point *= 2;
            //TODO HOEZO WERKT DIT? WAAROM MOET IK HET KEER 2 DOEN?

            var m = renderTarget.ViewMatrix * renderTarget.ProjectionMatrix;
            if (!Matrix4x4.Invert(m, out var inverted)) return point;

            return Vector2.Transform(point, inverted);
        }

        public override void StartLoop()
        {
            HookIntoEvents();
            window.Run();
        }

        public override void ResetInputState()
        {
            inputHandler.Reset();
        }

        private void HookIntoEvents()
        {
            window.Closing += OnWindowClose;
            window.Resize += OnWindowResize;
            window.Move += OnWindowMove;
            window.FileDrop += OnFileDrop;

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

            renderTarget.Initialise();
        }

        private void OnRenderFrame(object sender, FrameEventArgs obj)
        {
            while (true)
            {
                var e = GL.GetError();
                if (e == ErrorCode.NoError)
                    break;
                Logger.Error(e, nameof(OpenTKWindow));
            }

            time.RenderDeltaTime = (float)obj.Time;
            time.SecondsSinceLoad = (float)stopwatch.Elapsed.TotalSeconds;

            Game.Scene?.RenderSystems();
            if (Game.DevelopmentMode)
                Game.DebugDraw.Render();

            Graphics.CurrentTarget = RenderTarget;

            Game.Profiling.Render();
            Game.Console.Render();

            RenderQueue.RenderAndReset(internalGraphics);

            //GL.Flush();
            //GL.Finish();
            window.SwapBuffers();
        }

        private void OnUpdateFrame(object sender, FrameEventArgs obj)
        {
            time.UpdateDeltaTime = (float)obj.Time;

            Game.Console.Update();
            if (!Game.Console.IsEatingInput)
                Game.Scene?.UpdateSystems();
            Game.Profiling.Update();

            inputHandler.Reset();
        }

        private void OnFileDrop(object sender, FileDropEventArgs obj)
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
    }
}
