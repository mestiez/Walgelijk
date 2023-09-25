using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Walgelijk;

public class Compositor : IDisposable
{
    public CompositorNode RootNode => rootNode;
    public CompositorNode OutputNode => outputNode;
    public bool ForceUpdateTargets = true;

    public bool Enabled = true;

    public RenderTextureFlags Flags
    {
        get => flags;

        set
        {
            flags = value;
            ForceUpdateTargets = true;
        }
    }

    private RootCompositorNode rootNode = new();
    private OutputCompositorNode outputNode = new();
    private RenderTextureFlags flags = RenderTextureFlags.HDR;
    private readonly Game game;
    private readonly Material blitMat = new Material(Material.DefaultTextured);

    public Compositor(Game game)
    {
        this.game = game;
        game.Window.OnResize += OnWindowResize;
        Clear();
    }

    private void OnWindowResize(object? sender, global::System.Numerics.Vector2 e)
    {
        ForceUpdateTargets = true;
    }

    public void Clear()
    {
        ForceUpdateTargets = true;
        rootNode.Output.ConnectTo(outputNode.Inputs[0]);
    }

    public void Prepare()
    {
        if (!Enabled)
            return;

        if (ForceUpdateTargets || rootNode.Source == null)
        {
            rootNode.Source?.Dispose();
            rootNode.Source = new RenderTexture(game.Window.Width, game.Window.Height, WrapMode.Clamp, FilterMode.Nearest, Flags);
        }

        ForceUpdateTargets = false;
        game.Window.Graphics.CurrentTarget = rootNode.Source;
    }

    public void Render(IGraphics graphics)
    {
        if (!Enabled)
            return;

        graphics.CurrentTarget = game.Window.RenderTarget;
        graphics.BlitFullscreenQuad(
            outputNode.Output.Read(game)!,
            graphics.CurrentTarget,
            (int)graphics.CurrentTarget.Size.X,
            (int)graphics.CurrentTarget.Size.Y,
            blitMat,
            "mainTex");
    }

    public void Dispose()
    {
        blitMat.Dispose();
    }

    private class OutputCompositorNode : CompositorNode
    {
        public OutputCompositorNode() : base(1) { }

        public override RenderTexture? Read(Game game) => Inputs[0].Read(game);

        public override void Dispose() { }
    }

    private class RootCompositorNode : CompositorNode
    {
        public RootCompositorNode() : base(0) { }

        public RenderTexture? Source;

        public override RenderTexture? Read(Game game) => Source;

        public override void Dispose()
        {
            Source?.Dispose();
        }
    }
}

public abstract class CompositorNode : IDisposable
{
    public readonly CompositorSocket[] Inputs;
    public readonly CompositorSocket Output;

    protected CompositorNode(int inputCount)
    {
        Inputs = new CompositorSocket[inputCount];
        Output = new(CompositorSocketType.Output, this);

        for (int i = 0; i < inputCount; i++)
            Inputs[i] = new CompositorSocket(CompositorSocketType.Input, this);
    }

    public abstract void Dispose();
    public abstract RenderTexture? Read(Game game);
}

public enum CompositorSocketType
{
    Input,
    Output
}

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

    public RenderTexture? Read(Game game)
    {
        switch (SocketType)
        {
            case CompositorSocketType.Input:
                return Connected?.Read(game) ?? throw new Exception("Input socket has null connection but Read was called");
            default:
            case CompositorSocketType.Output:
                return Node.Read(game);
        }
    }
}