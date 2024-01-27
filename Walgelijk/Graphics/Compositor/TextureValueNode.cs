namespace Walgelijk;

public class TextureValueNode : CompositorNode
{
    public IReadableTexture? Texture;

    private RenderTexture? rt;
    private Material mat = new Material();

    public TextureValueNode(IReadableTexture? texture = null) : base(0)
    {
        Texture = texture;
    }

    public override RenderTexture? Read(Game game, in int w, in int h)
    {
        int width = w;
        int height = h;
        if (rt == null || rt.Width != w || rt.Height != h)
        {
            rt?.Dispose();
            var flags = (Texture?.HDR ?? false) ? RenderTargetFlags.HDR : RenderTargetFlags.None;
            //flags |= RenderTextureFlags.Multisampling;
            rt = new RenderTexture(w, h, flags: flags);
            rt.ProjectionMatrix = rt.OrthographicMatrix;
        }

        game.Window.Graphics.ActOnTarget(rt, g =>
        {
            g.Clear(Colors.Black);
            var t = Texture ?? Walgelijk.Texture.ErrorTexture;
            mat.SetUniform("mainTex", t);
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