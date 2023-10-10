using System.Numerics;

namespace Walgelijk.Mock;

/// <summary>
/// Empty render target implementation for testing purposes
/// </summary>
public class MockRenderTarget : RenderTarget
{
    public override Vector2 Size { get; set; }
}
