using System;
using System.Collections.Generic;
using System.Drawing;

namespace Walgelijk;

public class Compositor
{
    public IReadOnlyList<CompositorPass> Passes => passes.AsReadOnly();

    public bool ForceUpdateTargets = true;

    private readonly List<CompositorPass> passes = new();
    private Game game;

    public Compositor(Game game)
    {
        this.game = game;
        game.Window.OnResize += OnWindowResize;
    }

    private void OnWindowResize(object? sender, global::System.Numerics.Vector2 e)
    {
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
        foreach (var pass in passes)
            if (pass.Enabled)
            {
                if (ForceUpdateTargets)
                    pass.RegenerateRenderTexture(out _, game.Window.Width, game.Window.Height);
                queue.Add(pass.PushTask, pass.InclusiveStart);
                queue.Add(pass.PopTask, pass.ExclusiveEnd);
            }

        ForceUpdateTargets = false;
    }
}

public class CompositorPass : IDisposable
{
    public readonly string Name;
    public bool Enabled = true;
    public readonly RenderOrder InclusiveStart = new RenderOrder(0, 0);
    public readonly RenderOrder ExclusiveEnd = RenderOrder.UI;
    public readonly List<CompositorProcess> Steps = new();

    private RenderTarget? previousRt;
    private RenderTexture? rt;
    private bool pushed;

    public readonly IRenderTask PushTask;
    public readonly IRenderTask PopTask;

    public CompositorPass(in string name, RenderOrder inclusiveStart, RenderOrder exclusiveEnd, params CompositorProcess[] steps)
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

    public void RegenerateRenderTexture(out RenderTarget target, int width, int height)
    {
        if (rt != null)
            rt.Dispose();

        target = rt = new RenderTexture(width, height, hdr: true);
    }

    private void Push(IGraphics graphics)
    {
        pushed = rt != null && graphics.CurrentTarget != null;
        if (rt != null)
        {
            previousRt = graphics.CurrentTarget;
            graphics.CurrentTarget = rt;

            graphics.Clear(Colors.Transparent);
        }
    }

    private void Pop(IGraphics graphics)
    {
        if (pushed && rt != null && previousRt != null)
        {
            graphics.CurrentTarget = previousRt;

            foreach (var step in Steps)
            {
                step.Process(graphics, rt, graphics.CurrentTarget);
            }

            previousRt = null;
            pushed = false;
        }
    }

    public void Dispose()
    {
        previousRt = null;
        if (rt != null)
            rt.Dispose();
    }
}

public abstract class CompositorProcess
{
    public readonly string Name;

    protected CompositorProcess(string name)
    {
        Name = name;
    }

    public abstract void Process(IGraphics graphics, RenderTexture src, RenderTarget dst);
}

public abstract class ShaderProcess : CompositorProcess
{
    protected ShaderProcess(string name) : base(name)
    {
    }

    public override void Process(IGraphics graphics, RenderTexture src, RenderTarget dst)
    {
        graphics.BlitFullscreenQuad(src, dst, src.Width, src.Height, Material, MainTextureUniform);
    }

    protected abstract string MainTextureUniform { get; }
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
}