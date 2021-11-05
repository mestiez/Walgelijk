using ImGuiNET;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Walgelijk.OpenTK;

namespace Walgelijk.DearImgui
{
    class Program
    {
        private static Game game;

        static void Main(string[] args)
        {
            game = new Game(
                new OpenTKWindow("imgui test", new Vector2(-1, -1), new Vector2(800, 600)),
                new OpenALAudioRenderer()
                );

            game.Window.TargetFrameRate = 0;
            game.Window.TargetUpdateRate = 0;
            game.Console.DrawConsoleNotification = true;
            game.Window.VSync = false;

            var scene = new Scene(game);
            game.Scene = scene;

            var camera = scene.CreateEntity();
            scene.AttachComponent(camera, new TransformComponent());
            scene.AttachComponent(camera, new CameraComponent { PixelsPerUnit = 1, OrthographicSize = 1 });

            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new CameraSystem());
            scene.AddSystem(new DearImguiSystem());

            game.Start();
        }
    }

    public class DearImguiSystem : Walgelijk.System
    {
        private static bool hasBeenInitialised = false;
        private static bool hasBeenDestroyed = false;
        private static VertexBuffer vertexBuffer;
        private static Shader shader;
        private static Texture fontTexture;

        private bool hasFrame = false;

        public override void Initialise()
        {
            if (hasBeenInitialised)
                return;
            Scene.Game.Window.OnClose += OnWindowClose;

            var context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;

            vertexBuffer = new VertexBuffer();
            vertexBuffer.Dynamic = true;
            vertexBuffer.Vertices = new Vertex[0];
            vertexBuffer.Indices = new uint[0];

            unsafe
            {
                io.Fonts.GetTexDataAsRGBA32(out byte* data, out int width, out int height, out int _bytesPerPixel);
                const int componentsPerPixel = 4;
                const int bytesPerComponent = 1;
                const int bytesPerPixel = componentsPerPixel * bytesPerComponent;
                int totalBytes = bytesPerPixel * width * height;

                global::System.Diagnostics.Debug.Assert(_bytesPerPixel == bytesPerPixel);

                var bytes = new Span<byte>(data, totalBytes);
                var pixels = new Color[width * height];
                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    byte r = bytes[i + 0];
                    byte g = bytes[i + 1];
                    byte b = bytes[i + 2];
                    byte a = bytes[i + 3];
                    pixels[i / bytesPerPixel] = new Color(r, g, b, a);
                }
                fontTexture = new Texture(width, height, pixels, false);
                Scene.Game.Window.Graphics.Upload(fontTexture);
                fontTexture.DisposeCPUCopy();
                if (Scene.Game.Window.Graphics.TryGetId(fontTexture, out var textureId))
                    io.Fonts.SetTexID((IntPtr)textureId);
                else
                    Logger.Error("The DearImgui font texture could not be loaded...");
                io.Fonts.ClearTexData();
            }

            shader = new Shader(DearImguiShaders.VertexShader, DearImguiShaders.FragmentShader);

            hasBeenInitialised = true;
        }

        private void OnWindowClose(object sender, EventArgs e)
        {
            if (hasBeenDestroyed)
                return;
            hasBeenDestroyed = true;

            ImGui.DestroyContext();
        }

        public override void Render()
        {
            if (hasFrame)
            {
                ImGui.ShowDemoWindow();

                ImGui.Render();

                var io = ImGui.GetIO();
                var drawData = ImGui.GetDrawData();
                for (int i = 0; i < drawData.CmdListsCount; i++)
                {
                    var list = drawData.CmdListsRange[i];

                    int vertexBufferSize = list.VtxBuffer.Size;
                    if (vertexBuffer.Vertices.Length < vertexBufferSize)
                        vertexBuffer.Vertices = new Vertex[vertexBufferSize];

                    int indexBufferSize = list.IdxBuffer.Size;
                    if (vertexBuffer.Indices.Length < indexBufferSize)
                        vertexBuffer.Indices = new uint[indexBufferSize];
                }

                for (int i = 0; i < drawData.CmdListsCount; i++)
                {
                    var list = drawData.CmdListsRange[i];
                    for (int vertexIndex = 0; vertexIndex < list.VtxBuffer.Size; vertexIndex++)
                    {
                        var vert = list.VtxBuffer[vertexIndex];
                        vertexBuffer.Vertices[i] = ImDrawVertToVertex(vert);
                    }                   
                    for (int indexIndex = 0; indexIndex < list.IdxBuffer.Size; indexIndex++)
                    {
                        var index = list.IdxBuffer[indexIndex];
                        vertexBuffer.Indices[i] = index;
                    }
                    vertexBuffer.ForceUpdate();
                    for (int p = 0; p < list.CmdBuffer.Size; p++)
                    {
                        var cmd = list.CmdBuffer[p];
                        if (cmd.UserCallback != IntPtr.Zero)
                            continue;

                        var clip = cmd.ClipRect;
                    //    Scene.Game.Window.Graphics.DrawBounds = new DrawBounds(new Vector2(clip.X, clip.Y), new Vector2(clip.Z, clip.W));
                        Scene.Game.Window.Graphics.DrawInstanced(vertexBuffer, (int)cmd.ElemCount, Material.DefaultTextured);
                    }
                }
            }

            UpdateIO();

            ImGui.NewFrame();
            hasFrame = true;
        }

        private unsafe static Vertex ImDrawVertToVertex(ImDrawVertPtr vert)
        {
            byte r = (byte)(vert.col << (8 * 0));
            byte g = (byte)(vert.col << (8 * 1));
            byte b = (byte)(vert.col << (8 * 2));
            byte a = (byte)(vert.col << (8 * 3));

            return new Vertex
            {
                Position = new Vector3(vert.pos.X, vert.pos.Y, 0),
                TexCoords = vert.uv,
                Color = new Color(r, g, b, a)
            };
        }

        private void UpdateIO()
        {
            var io = ImGui.GetIO();

            io.DisplaySize = Scene.Game.Window.Size;
            io.DeltaTime = Time.RenderDeltaTime;

            io.MouseDown[0] = Input.IsButtonHeld(Button.Left);
            io.MouseDown[1] = Input.IsButtonHeld(Button.Middle);
            io.MouseDown[2] = Input.IsButtonHeld(Button.Right);

            io.MousePos = Input.WindowMousePosition;

            foreach (Key key in Enum.GetValues(typeof(Key)))
                if (key != Key.Unknown)
                    io.KeysDown[(int)key] = Input.IsKeyHeld(key);

            for (int i = 0; i < Input.TextEntered.Length; i++)
                io.AddInputCharacter(Input.TextEntered[i]);

            io.KeyCtrl = Input.IsKeyHeld(Key.LeftControl) || Input.IsKeyHeld(Key.RightControl);
            io.KeyAlt = Input.IsKeyHeld(Key.LeftAlt) || Input.IsKeyHeld(Key.RightAlt);
            io.KeyShift = Input.IsKeyHeld(Key.LeftShift) || Input.IsKeyHeld(Key.RightShift);
            io.KeySuper = Input.IsKeyHeld(Key.LeftSuper) || Input.IsKeyHeld(Key.RightSuper);

            io.MouseWheel = Input.MouseScrollDelta;
        }
    }

    public struct DearImguiShaders
    {
        public const string VertexShader = @"#version 330 core
uniform mat4 projection_matrix;
layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;
out vec4 color;
out vec2 texCoord;
void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";

        public const string FragmentShader = @"#version 330 core
uniform sampler2D in_fontTexture;
in vec4 color;
in vec2 texCoord;
out vec4 outputColor;
void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";
    }
}
