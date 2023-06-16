using System;
using System.Collections.Generic;

namespace Walgelijk;

public class Compositor
{
    public IReadOnlyList<CompositorPass> Passes => passes.AsReadOnly();

    public bool ForceUpdateTargets = true;

    private RenderTexture? buffer;
    private readonly List<CompositorPass> passes = new();
    private Game game;
    private Material blitMat = new Material();

    private IRenderTask targetBufferTask;
    private IRenderTask targetWindowTask;
    private IRenderTask blitBufferTask;
    private ulong framesRendered = 0;

    public Compositor(Game game)
    {
        this.game = game;
        game.Window.OnResize += OnWindowResize;

        buffer = new RenderTexture(game.Window.Width, game.Window.Height, hdr: true);
        targetBufferTask = new TargetRenderTask(buffer);
        targetWindowTask = new TargetRenderTask(game.Window.RenderTarget);
        blitBufferTask = new ActionRenderTask(BlitBuffer);
    }

    private void OnWindowResize(object? sender, global::System.Numerics.Vector2 e)
    {
        ForceUpdateTargets = true;
    }

    public void Reset()
    {
        passes.Clear();
        ForceUpdateTargets = true;
    }

    public void AddPass(CompositorPass pass)
    {
        if (passes.Contains(pass))
            throw new Exception("This pass is already present in the pass list");

        passes.Add(pass);
        ForceUpdateTargets = true;
    }

    public bool RemovePass(CompositorPass pass)
    {
        ForceUpdateTargets = true;
        return passes.Remove(pass);
    }

    public void Render(RenderQueue queue)
    {
        if (passes.Count == 0)
            return;

        if (framesRendered != 0 && (ForceUpdateTargets || buffer == null))
        {
            buffer?.Dispose();
            buffer = new RenderTexture(game.Window.Width, game.Window.Height, hdr: true);
            targetBufferTask = new TargetRenderTask(buffer);
        }

        // queue.Add(targetBufferTask, passes.Min(static p => p.InclusiveStart));

        foreach (var pass in passes)
        {
            if (pass.Enabled)
            {
                if (ForceUpdateTargets)
                    pass.RegenerateRenderTextures(game.Window.Width, game.Window.Height);
                queue.Add(pass.PushTask, pass.InclusiveStart);
                queue.Add(pass.PopTask, pass.ExclusiveEnd);
            }
        }

        //   var end = passes.Max(static p => p.ExclusiveEnd);
        //  queue.Add(targetWindowTask, end);
        //  queue.Add(blitBufferTask, end);

        ForceUpdateTargets = false;

        framesRendered++;
    }

    private void BlitBuffer(IGraphics g)
    {
        if (buffer != null)
            g.BlitFullscreenQuad(buffer, game.Window.RenderTarget, game.Window.Width, game.Window.Height, blitMat, ShaderDefaults.MainTextureUniform);
    }
}

public class CompositorPass : IDisposable
{
    public readonly string Name;
    public bool Enabled = true;
    public readonly RenderOrder InclusiveStart = new RenderOrder(0, 0);
    public readonly RenderOrder ExclusiveEnd = RenderOrder.UI;
    public readonly List<CompositorStep> Steps = new();

    private Material blitMat = new Material();

    private RenderTarget? finalTarget;

    private RenderTexture? rtSrc;
    private RenderTexture? rtDst;
    private bool pushed;

    public readonly IRenderTask PushTask;
    public readonly IRenderTask PopTask;

    public CompositorPass(in string name, RenderOrder inclusiveStart, RenderOrder exclusiveEnd, params CompositorStep[] steps)
    {
        if (inclusiveStart >= exclusiveEnd)
            throw new Exception("Start layer cannot be more than or equal to end layer");

        Name = name;
        InclusiveStart = inclusiveStart;
        ExclusiveEnd = exclusiveEnd;
        Steps.AddRange(steps);

        PushTask = new ActionRenderTask(Push);
        PopTask = new ActionRenderTask(Pop);
    }

    public void RegenerateRenderTextures(int width, int height)
    {
        rtSrc?.Dispose();
        rtSrc = new RenderTexture(width, height, hdr: true);

        rtDst?.Dispose();
        rtDst = new RenderTexture(width, height, hdr: true);
    }

    private void Push(IGraphics graphics)
    {
        pushed = rtSrc != null && rtDst != null && graphics.CurrentTarget != null;
        if (rtSrc != null && rtDst != null)
        {
            // TODO je neemt aan dat graphics.CurrentTarget 1 pass is of de window. elke pass moet zn eigen target hebben :) en maak steps disposable
            finalTarget = graphics.CurrentTarget;
            if (finalTarget != null)
            {
                rtDst.ViewMatrix = rtSrc.ViewMatrix = finalTarget.ViewMatrix;
                rtDst.ProjectionMatrix = rtSrc.ProjectionMatrix = finalTarget.ProjectionMatrix;
            }
            graphics.CurrentTarget = rtSrc;
            graphics.Clear(Colors.Transparent);
        }
    }

    private void Pop(IGraphics graphics)
    {
        var state = Game.Main.State; //TODO dit is niet al te best
        if (pushed && rtSrc != null && rtDst != null && finalTarget != null)
        {
            graphics.CurrentTarget = rtDst;
            graphics.Clear(Colors.Transparent);
            //graphics.Blit(rtSrc, rtDst);

            foreach (var step in Steps)
            {
                step.Process(graphics, rtSrc, rtDst, state);
                graphics.Blit(rtDst, rtSrc);
            }

            graphics.CurrentTarget = finalTarget;
            graphics.BlitFullscreenQuad(rtDst, finalTarget, rtDst.Width, rtDst.Height, blitMat, ShaderDefaults.MainTextureUniform);
            finalTarget = null;
            pushed = false;
        }
    }

    public void Dispose()
    {
        finalTarget = null;
        blitMat.Dispose();
        rtSrc?.Dispose();
        rtDst?.Dispose();
    }
}

public abstract class CompositorStep : IDisposable
{
    public readonly string Name;

    protected CompositorStep(string name)
    {
        Name = name;
    }

    public abstract void Dispose();

    public abstract void Process(IGraphics graphics, RenderTexture src, RenderTexture dst, GameState state);
}

public abstract class ShaderProcess : CompositorStep
{

    protected ShaderProcess(string name) : base(name)
    {
    }

    public override void Process(IGraphics graphics, RenderTexture src, RenderTexture dst, GameState state)
    {
        if (!string.IsNullOrEmpty(TimeFloatUniform))
            Material.SetUniform(TimeFloatUniform, state.Time.SecondsSinceLoad);

        graphics.BlitFullscreenQuad(src, dst, src.Width, src.Height, Material, MainTextureUniform);
    }

    protected abstract string MainTextureUniform { get; }
    protected abstract string? TimeFloatUniform { get; }
    protected abstract Material Material { get; }
}

public class InvertProcess : ShaderProcess
{
    private Material mat;

    public InvertProcess(string name = nameof(InvertProcess)) : base(name)
    {
        mat = new Material(new Shader(Shader.Default.VertexShader,
@"#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    vec4 c = texture(mainTex, uv);
    c.rgb = 1 - c.rgb;
    color = vertexColor * c;
}"
));
    }

    protected override Material Material => mat;
    protected override string MainTextureUniform => "mainTex";
    protected override string? TimeFloatUniform => null;

    public override void Dispose()
    {
        mat.Dispose();
    }
}

public class BlinkProcess : ShaderProcess
{
    private Material mat;

    public BlinkProcess(string name = nameof(BlinkProcess)) : base(name)
    {
        mat = new Material(new Shader(Shader.Default.VertexShader,
@"#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;
uniform float time;

void main()
{
    vec4 c = texture(mainTex, uv);
    c *= mod(time + uv.x, 1) > 0.5 ? 0 : 1;
    color = vertexColor * c;
}"
));
    }

    protected override Material Material => mat;
    protected override string MainTextureUniform => "mainTex";
    protected override string? TimeFloatUniform => "time";

    public override void Dispose()
    {
        mat.Dispose();
    }
}