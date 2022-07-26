using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK;

public class OpenTKWindow : Window
{
    public override string Title { get => window.Title; set => window.Title = value; }
    public override Vector2 Position { get => new Vector2(window.Location.X, window.Location.Y); set => window.Location = new global::OpenTK.Mathematics.Vector2i((int)value.X, (int)value.Y); }
    public override int TargetUpdateRate { get; set; }
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
        set => window.Size = new global::OpenTK.Mathematics.Vector2i((int)value.X, (int)value.Y);
    }

    internal readonly Time time = new();
    internal readonly NativeWindow window;
    internal readonly OpenTKWindowRenderTarget renderTarget;
    internal readonly InputHandler inputHandler;
    internal readonly OpenTKGraphics internalGraphics;
    private readonly Stopwatch clock = new();

    public OpenTKWindow(string title, Vector2 position, Vector2 size)
    {
        window = new NativeWindow(new NativeWindowSettings
        {
            Size = new global::OpenTK.Mathematics.Vector2i((int)size.X, (int)size.Y),
            Title = title,
            StartVisible = false,
            NumberOfSamples = 0,
        });

        if (position.X >= 0 && position.Y >= 0)
            Position = position;
        else
            window.CenterWindow();

        renderTarget = new OpenTKWindowRenderTarget();
        renderTarget.Window = this;

        inputHandler = new InputHandler(this);
        internalGraphics = new OpenTKGraphics();

        Logger.Log("Graphics API: " + window.API.ToString());
    }

    public override void Close() => window.Close();

    public override void ResetInputState() => inputHandler.Reset();

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

        static void getCoords(int index, out int x, out int y)
        {
            x = index % res;
            y = (int)MathF.Floor(index / res);
        }

        window.Icon = new WindowIcon(new global::OpenTK.Windowing.Common.Input.Image(res, res, icon));
    }

    public override void StartLoop()
    {
        //window.Run();
        window.MakeCurrent();
        window.IsVisible = true;
        OnWindowLoad();
        OnWindowResize(new ResizeEventArgs(window.Size));
        window.Move += OnWindowMove;
        window.Resize += OnWindowResize;
        clock.Start();
        while (!window.IsExiting)
        //while (!global::OpenTK.Windowing.GraphicsLibraryFramework.GLFW.WindowShouldClose(window.WindowPtr))
        {
            var elapsed = clock.Elapsed.TotalSeconds;
            clock.Restart();
            window.ProcessInputEvents();
            NativeWindow.ProcessWindowEvents(window.IsEventDriven);
            Update(elapsed);

            if (TargetUpdateRate > float.Epsilon)
            {
                double toWait = (1d / TargetUpdateRate) - elapsed;
                if (toWait > 0)
                    global::System.Threading.Thread.Sleep(TimeSpan.FromSeconds(toWait ));
            }
        }
        window.IsVisible = false;
        OnWindowClose();
    }

    internal void Update(double dt)
    {
        //update
        {
            var unscaledDt = (float)dt;
            var scaledDt = (float)dt * Time.TimeScale;

            time.DeltaTimeUnscaled = unscaledDt;
            time.DeltaTime = scaledDt;

            time.SecondsSinceSceneChange += unscaledDt;
            time.SecondsSinceSceneChangeUnscaled += scaledDt;

            time.SecondsSinceLoad += scaledDt;
            time.SecondsSinceLoadUnscaled += unscaledDt;

            Game.AudioRenderer.UpdateTracks();
            Game.Console.Update();
            if (!Game.Console.IsActive)
                Game.Scene?.UpdateSystems();
            Game.Profiling.Tick();
            Game.AudioRenderer.Process(Game);

            inputHandler.Reset();
        }

        //render
        {
            GLUtilities.PrintGLErrors(Game.Main.DevelopmentMode);
            Game.Scene?.RenderSystems();
            if (Game.DevelopmentMode)
                Game.DebugDraw.Render();
            Graphics.CurrentTarget = RenderTarget;
            Game.Console.Render();
            RenderQueue.RenderAndReset(internalGraphics);
            window.Context.SwapBuffers();
        }
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
        if (!Matrix4x4.Invert(m, out var inverted))
            return point;

        return Vector2.Transform(point, inverted);
    }

    internal void OnWindowLoad()
    {
        RenderTarget.Size = Size;
        renderTarget.Initialise();

        GL.DebugMessageCallback(OnGLDebugMessage, IntPtr.Zero);
    }

    internal void OnWindowClose() => InvokeCloseEvent();

    internal void OnFileDropped(FileDropEventArgs obj) => InvokeFileDropEvent(obj.FileNames);

    internal void OnWindowMove(WindowPositionEventArgs e) => InvokeMoveEvent(new Vector2(e.Position.X, e.Position.Y));

    internal void OnWindowResize(ResizeEventArgs e)
    {
        RenderTarget.Size = new Vector2(e.Width, e.Height);
        InvokeResizeEvent(Size);
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
}
