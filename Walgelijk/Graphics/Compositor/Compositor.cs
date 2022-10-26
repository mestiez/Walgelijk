using System;
using System.Collections.Generic;

namespace Walgelijk;

public class Compositor
{
    private Game game;
    private RenderTexture? rt;

    public readonly List<CompositorPass> Passes = new();

    public Compositor(Game game)
    {
        this.game = game;
    }

    public void RegenerateRenderTexture(out RenderTarget target)
    {
        if (rt != null)
            rt.Dispose();

        target = rt = new RenderTexture(game.Window.Width, game.Window.Height, hdr: true);
    }

    public void StartPass(RenderQueue queue, int passIndex)
    {
        if (passIndex < 0 || passIndex >= Passes.Count)
            throw new global::System.IndexOutOfRangeException();

        RenderTarget? target;
        if (rt == null)
            RegenerateRenderTexture(out target);
        else 
            target = rt;

        queue.Add(new TargetRenderTask(target));
    }

    public void EndPass()
}

public class CompositorPass
{
    public readonly string Name;
    public bool Enabled = true;

    public readonly RenderOrder InclusiveStart = new RenderOrder(0, 0);
    public readonly RenderOrder ExclusiveEnd = RenderOrder.UI;

    public readonly List<CompositorProcess> Steps = new();

    public CompositorPass(in string name, RenderOrder inclusiveStart, RenderOrder exclusiveEnd, params CompositorProcess[] steps)
    {
        if (inclusiveStart >= exclusiveEnd)
            throw new Exception("Start layer cannot be more than or equal to end layer");

        Name = name;
        InclusiveStart = inclusiveStart;
        ExclusiveEnd = exclusiveEnd;
        Steps.AddRange(steps);
    }
}

public abstract class CompositorProcess
{
    public readonly string Name;

    protected CompositorProcess(string name)
    {
        Name = name;
    }

    public abstract void Process(IGraphics graphics, RenderTexture src, RenderTexture dst);
}

public abstract class ShaderProcess : CompositorProcess
{
    protected ShaderProcess(string name) : base(name)
    {
    }

    public override void Process(IGraphics graphics, RenderTexture src, RenderTexture dst)
    {
        graphics.BlitFullscreenQuad(src, dst, Material, MainTextureUniform);
    }

    protected abstract string MainTextureUniform { get; }
    protected abstract Material Material { get; }
}

public class InvertProcess : ShaderProcess
{
    private Material mat;

    public InvertProcess(string name) : base(name)
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