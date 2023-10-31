using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Walgelijk;

public class Compositor : IDisposable
{
    public CompositorNode SourceNode => sourceNode;
    public CompositorNode DestinationNode => destinationNode;
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

    private RootCompositorNode sourceNode = new();
    private OutputCompositorNode destinationNode = new();
    private RenderTextureFlags flags = RenderTextureFlags.HDR;
    private readonly Material blitMat = new Material(Material.DefaultTextured);
    private readonly Game game;

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
        sourceNode.Output.ConnectTo(destinationNode.Inputs[0]);
    }

    public void Prepare()
    {
        if (!Enabled || game.Window.Width == 0 || game.Window.Height == 0)
            return;

        if (ForceUpdateTargets || sourceNode.Source == null)
        {
            sourceNode.Source?.Dispose();
            sourceNode.Source = new RenderTexture(game.Window.Width, game.Window.Height, WrapMode.Clamp, FilterMode.Nearest, Flags);
        }

        ForceUpdateTargets = false;
        game.Window.Graphics.CurrentTarget = sourceNode.Source;
    }

    public void Render(IGraphics graphics)
    {
        if (!Enabled)
            return;

        int w = (int)graphics.CurrentTarget.Size.X;
        int h = (int)graphics.CurrentTarget.Size.Y;
        graphics.CurrentTarget = game.Window.RenderTarget;
        graphics.BlitFullscreenQuad(
            destinationNode.Output.Read(game, w, h)!,
            graphics.CurrentTarget,
            w, h,
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
        public override RenderTexture? Read(Game game, in int width, in int height) => Inputs[0].Read(game, width, height);
        public override void Dispose() { }
    }

    private class RootCompositorNode : CompositorNode
    {
        public RootCompositorNode() : base(0) { }
        public RenderTexture? Source;
        public override RenderTexture? Read(Game game, in int width, in int height) => Source;

        public override void Dispose() => Source?.Dispose();
    }
}
