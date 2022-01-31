using Walgelijk;

namespace Videogame;

public struct RenderOrders
{
    public static readonly RenderOrder Default = new(0, 0);
    public static readonly RenderOrder Imgui   = new(100, 0);
}
