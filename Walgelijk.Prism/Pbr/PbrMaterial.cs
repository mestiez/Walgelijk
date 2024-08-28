namespace Walgelijk.Prism.Pbr;

public class PbrMaterial : IDisposable
{
    public static Texture BlackPixel = TexGen.Colour(1, 1, Colors.Black);
    public static Texture GrayPixel = TexGen.Colour(1, 1, Colors.Gray);
    public static Texture UpNormalPixel = TexGen.Colour(1, 1, new Color(0x8080FF));

    public static Texture Checkerboard = TexGen.Checkerboard(256, 256, 1, Colors.Gray, Colors.White);

    static PbrMaterial()
    {
        BlackPixel.WrapMode = WrapMode.Repeat;
        GrayPixel.WrapMode = WrapMode.Repeat;
        UpNormalPixel.WrapMode = WrapMode.Repeat;
    }

    private readonly Material material;

    public Material Material => material;

    public IReadableTexture AlbedoMap
    {
        get => material.InternalUniforms.Textures["albedoMap"];
        set => material.SetUniform("albedoMap", value);
    }

    public IReadableTexture RoughnessMap
    {
        get => material.InternalUniforms.Textures["roughnessMap"];
        set => material.SetUniform("roughnessMap", value);
    }

    public IReadableTexture MetallicMap
    {
        get => material.InternalUniforms.Textures["metallicMap"];
        set => material.SetUniform("metallicMap", value);
    }

    public IReadableTexture NormalMap
    {
        get => material.InternalUniforms.Textures["normalMap"];
        set => material.SetUniform("normalMap", value);
    }

    public IReadableTexture WorldReflection
    {
        get => material.InternalUniforms.Textures["world"];
        set => material.SetUniform("world", value);
    }

    public Color Tint
    {
        get => material.InternalUniforms.Vec4s["tint"];
        set => material.SetUniform("tint", value);
    }

    public PbrMaterial()
    {
        material = new Material(BuiltInShaders.Pbr);
        material.SetUniform("albedoMap", Checkerboard);
        material.SetUniform("roughnessMap", Texture.White);
        material.SetUniform("metallicMap", BlackPixel);
        material.SetUniform("normalMap", UpNormalPixel);
        material.SetUniform("world", Resources.Load<Texture>("env.png"));
        material.SetUniform("tint", Colors.White);
        material.BackfaceCulling = true;
        material.DepthTested = true;
    }

    public void Dispose()
    {
        material.Dispose();
    }

    public static implicit operator Material(PbrMaterial mat) => mat.material;
}
