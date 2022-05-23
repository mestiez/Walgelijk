using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
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

        private readonly Time time = new();
        private Stopwatch stopwatch;

        public OpenTKWindow(string title, Vector2 position, Vector2 size)
        {
            GameWindowSettings windowSettings = new();

            window = new GameWindow(windowSettings, new NativeWindowSettings
            {
                Size = new global::OpenTK.Mathematics.Vector2i((int)size.X, (int)size.Y),
                Title = title,
                StartVisible = false,
                NumberOfSamples = 16
            });

            if (position.X >= 0 && position.Y >= 0)
                Position = position;

            renderTarget = new OpenTKWindowRenderTarget();
            renderTarget.Window = this;

            inputHandler = new InputHandler(this);
            internalGraphics = new OpenTKGraphics();

            Logger.Log("Graphics API: " + window.API.ToString());
        }

        public override string Title { get => window.Title; set => window.Title = value; }
        public override Vector2 Position { get => new Vector2(window.Location.X, window.Location.Y); set => window.Location = new global::OpenTK.Mathematics.Vector2i((int)value.X, (int)value.Y); }
        public override int TargetUpdateRate
        {
            get
            {
                return (int)window.UpdateFrequency;
            }
            set
            {
                window.RenderFrequency = window.UpdateFrequency = value;
            }
        }
        public override bool VSync { get => window.VSync == VSyncMode.On; set => window.VSync = (value ? VSyncMode.On : VSyncMode.Off); }
        public override bool IsOpen => window.Exists && !window.IsExiting;
        public override bool HasFocus => window.IsFocused;
        public override Time Time => time;
        public override bool IsVisible { get => window.IsVisible; set => window.IsVisible = value; }
        public override bool Resizable { get => window.WindowBorder == WindowBorder.Resizable; set => window.WindowBorder = value ? WindowBorder.Resizable : WindowBorder.Fixed; }
        public override InputState InputState => inputHandler?.InputState ?? default;
        public override RenderTarget RenderTarget => renderTarget;
        public override IGraphics Graphics => internalGraphics;

        public override Vector2 Size
        {
            get => new(window.Size.X, window.Size.Y);
            set
            {
                window.Size = new global::OpenTK.Mathematics.Vector2i((int)value.X, (int)value.Y);
            }
        }

        public override void Close()
        {
            window.Close();
        }

        public override void SetIcon(IReadableTexture texture, bool flipY = true)
        {
            if (texture.Width != texture.Height || texture.Width != 32)
                throw new Exception("The window icon resolution has to be 32x32");

            const int res = 32;
            var icon = new byte[res * res * 4];

            for (int i = 0; i < icon.Length; i += 4)
            {
                getCoords(i / 4, out int x, out int y);
                var pixel = texture.GetPixel(x, flipY ? res - 1 - y : y);
                var bytes = pixel.ToBytes();
                icon[i + 0] = bytes.r;
                icon[i + 1] = bytes.g;
                icon[i + 2] = bytes.b;
                icon[i + 3] = bytes.a;
            }

            void getCoords(int index, out int x, out int y)
            {
                x = index % res;
                y = (int)MathF.Floor(index / res);
            }

            window.Icon = new WindowIcon(new global::OpenTK.Windowing.Common.Input.Image(res, res, icon));
        }

        public override Vector2 ScreenToWindowPoint(Vector2 point)
        {
            var pos = window.PointToClient(new global::OpenTK.Mathematics.Vector2i((int)point.X, (int)point.Y));
            return new Vector2(pos.X, pos.Y);
        }

        public override Vector2 WindowToScreenPoint(Vector2 point)
        {
            var pos = window.PointToScreen(new global::OpenTK.Mathematics.Vector2i((int)point.X, (int)point.Y));
            return new Vector2(pos.X, pos.Y);
        }

        //TODO dit is eigenlijk iets van de rendertarget
        public override Vector2 WorldToWindowPoint(Vector2 world)
        {
            var p = Vector2.Transform(world, renderTarget.ViewMatrix * renderTarget.ProjectionMatrix);

            p /= 2;
            p.Y = 1 - p.Y;
            p.Y -= 0.5f;
            p.X += 0.5f;
            p *= renderTarget.Size;

            return p;
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
            window.Closed += OnWindowClose;
            window.Resize += OnWindowResize;
            window.Move += OnWindowMove;
            window.FileDrop += OnFileDropped;

            window.UpdateFrame += OnUpdateFrame;
            window.RenderFrame += OnRenderFrame;

            window.Load += OnWindowLoad;

            GL.DebugMessageCallback(OnGLDebugMessage, IntPtr.Zero);
        }

        private void OnGLDebugMessage(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            var str = Marshal.PtrToStringAuto(message, length);
            switch (severity)
            {
                case DebugSeverity.DebugSeverityHigh:
                    Logger.Error(str, nameof(OpenTKWindow));
                    break;
                case DebugSeverity.DebugSeverityMedium:
                    Logger.Warn(str, nameof(OpenTKWindow));
                    break;
                default:
                case DebugSeverity.DontCare:
                case DebugSeverity.DebugSeverityNotification:
                case DebugSeverity.DebugSeverityLow:
                    Logger.Log(str, nameof(OpenTKWindow));
                    break;
            }
        }

        private void OnWindowLoad()
        {
            window.IsVisible = true;

            window.MakeCurrent();
            RenderTarget.Size = Size;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            renderTarget.Initialise();
        }

        private void OnRenderFrame(FrameEventArgs obj)
        {
            GLUtilities.PrintGLErrors(Game.Main.DevelopmentMode);

            Game.Scene?.RenderSystems();
            if (Game.DevelopmentMode)
                Game.DebugDraw.Render();

            Graphics.CurrentTarget = RenderTarget;

            //Game.Profiling.Render();
            Game.Console.Render();

            RenderQueue.RenderAndReset(internalGraphics);

            //GL.Flush();
            //GL.Finish();
            window.SwapBuffers();
        }

        private void OnUpdateFrame(FrameEventArgs obj)
        {
            Game.Console.Update();
            if (!Game.Console.IsActive)
                Game.Scene?.UpdateSystems();
            Game.Profiling.Tick();
            Game.AudioRenderer.Process(Game);

            inputHandler.Reset();

            var now = stopwatch.Elapsed.TotalSeconds;

            time.DeltaTimeUnscaled = (float)(now - time.SecondsSinceLoadUnscaled);
            time.DeltaTime = time.DeltaTimeUnscaled * Time.TimeScale;

            time.SecondsSinceSceneChange += time.DeltaTime;
            time.SecondsSinceSceneChangeUnscaled += time.DeltaTimeUnscaled;

            time.SecondsSinceLoad += time.DeltaTime;
            time.SecondsSinceLoadUnscaled = (float)now;
        }

        private void OnFileDropped(FileDropEventArgs obj)
        {
            InvokeFileDropEvent(obj.FileNames);
        }

        private void OnWindowMove(WindowPositionEventArgs e)
        {
            InvokeMoveEvent(new Vector2(e.Position.X, e.Position.Y));
        }

        private void OnWindowResize(ResizeEventArgs e)
        {
            RenderTarget.Size = new Vector2(e.Width, e.Height);
            InvokeResizeEvent(Size);
        }

        private void OnWindowClose()
        {
            InvokeCloseEvent();
        }
    }
}
