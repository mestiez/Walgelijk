using System.Numerics;

namespace Walgelijk.Mock;

/// <summary>
/// Empty window implementation for testing purposes
/// </summary>
public class MockWindow : Window
{
    public override string Title { get; set; } = "Fake window";
    public override Vector2 Position { get; set; }
    public override Vector2 Size { get; set; }
    public override bool VSync { get; set; }

    public override bool IsOpen { get; }

    public override bool HasFocus => false;

    public override bool IsVisible { get; set; }
    public override bool Resizable { get; set; }
    public override WindowType WindowType { get; set; }

    public override IGraphics Graphics => new MockGraphics();

    public override float DPI => 96;

    public override bool IsCursorLocked { get; set; }
    public override DefaultCursor CursorAppearance { get; set; }
    public override IReadableTexture? CustomCursor { get; set; }

    public override RenderTarget RenderTarget => new MockRenderTarget();

    public override void Close() { }

    public override void Deinitialise() { }

    public override void Initialise() { }

    public override void LoopCycle() { }

    public override void ResetInputState() { }

    public override Vector2 ScreenToWindowPoint(Vector2 screen) => screen;

    public override void SetIcon(IReadableTexture texture, bool flipY = true) { }

    public override Vector2 WindowToScreenPoint(Vector2 window) => window;

    public override Vector2 WindowToWorldPoint(Vector2 window) => window;

    public override Vector2 WorldToWindowPoint(Vector2 world) => world;
}