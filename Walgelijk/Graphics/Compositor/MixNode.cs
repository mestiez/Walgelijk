using System;

namespace Walgelijk;

public class MixNode : CompositorNode
{
    public float Factor = 0.5f;

    private RenderTexture? rt;
    private readonly Material mat;

    private const string fragment =
@"#version 330 core

in vec2 uv;
in vec4 vertexColor;
in vec3 normal;

out vec4 color;

uniform sampler2D texA;
uniform sampler2D texB;
uniform float factor;

void main()
{
    color = vertexColor * mix(texture(texA, uv), texture(texB, uv), factor);
}";

    private static readonly Shader shader = new Shader(ShaderDefaults.WorldSpaceVertex, fragment);

    public MixNode() : base(2)
    {
        mat = new Material(shader);
    }

    public override RenderTexture? Read(Game game, in int w, in int h)
    {
        int width = w;
        int height = h;
        var a = Inputs[0].Read(game, w, h) ?? throw new NullReferenceException("MixNode input 0 is null");
        var b = Inputs[1].Read(game, w, h) ?? throw new NullReferenceException("MixNode input 1 is null");

        if (rt == null || rt.Width != w || rt.Height != h)
        {
            rt?.Dispose();
            // TODO voor onbekende redenen werkt het niet als 'je HDR aan doet
            rt = new RenderTexture(w, h, flags: a.Flags & b.Flags);
            rt.ProjectionMatrix = rt.OrthographicMatrix;
        }

        game.Window.Graphics.ActOnTarget(rt, g =>
        {
            mat.SetUniform("texA", a);
            mat.SetUniform("texB", b);
            mat.SetUniform("factor", Factor);
            g.DrawQuad(new Rect(0, 0, width, height), mat);
        });

        return rt;
    }

    public override void Dispose()
    {
        rt?.Dispose();
        mat.Dispose();
    }
}
