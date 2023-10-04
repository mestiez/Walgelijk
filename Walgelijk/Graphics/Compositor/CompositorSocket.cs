using System;

namespace Walgelijk;

public class CompositorSocket
{
    public readonly CompositorSocketType SocketType;
    public readonly CompositorNode Node;
    public CompositorSocket? Connected { get; private set; }

    public CompositorSocket(CompositorSocketType socketType, CompositorNode node)
    {
        SocketType = socketType;
        Node = node;
    }

    public void ConnectTo(CompositorSocket other)
    {
        if (other.SocketType == SocketType)
            throw new InvalidOperationException(
                $"Attempt to connect {SocketType} {nameof(CompositorSocket)} to {other.SocketType} {nameof(CompositorSocket)}, which is invalid");

        other.Connected = this;
        Connected = other;
    }

    public RenderTexture? Read(Game game, in int width, in int height)
    {
        switch (SocketType)
        {
            case CompositorSocketType.Input:
                return Connected?.Read(game, width, height) ?? throw new Exception("Input socket has null connection but Read was called");
            default:
            case CompositorSocketType.Output:
                return Node.Read(game, width, height);
        }
    }
}
