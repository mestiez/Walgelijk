using System;

namespace Walgelijk;

/// <summary>
/// Utility struct that provides static text material creation functions
/// </summary>
public static class FontMaterialCreator
{
    public static readonly Shader BitmapShader = new(BuiltInShaders.WorldSpaceVertex, BuiltInShaders.Text.BitmapFragment);
    public static readonly Shader SdfShader = new(BuiltInShaders.WorldSpaceVertex, BuiltInShaders.Text.SdfFragment);
    public static readonly Shader MsdfShader = new(BuiltInShaders.WorldSpaceVertex, BuiltInShaders.Text.MsdfFragment);

    /// <summary>
    /// Create a material for a given font
    /// </summary>
    /// <param name="font"></param>
    /// <returns></returns>
    public static Material CreateFor(Font font)
    {
        return font.Rendering switch
        {
            FontRendering.Bitmap => CreateBitmapFontMaterial(font.Page),
            FontRendering.SDF => CreateSDFMaterial(font.Page),
            FontRendering.MSDF => CreateMSDFMaterial(font.Page),
            _ => throw new ArgumentException("Invalid FontRendering value: only Bitmap, SDF, and MSDF are supported by this function"),
        };
    }

    /// <summary>
    /// Create a material for simple bitmap fonts
    /// </summary>
    public static Material CreateBitmapFontMaterial(IReadableTexture page)
    {
        Material mat = new Material(BitmapShader);

        mat.SetUniform("mainTex", page);
        mat.SetUniform("tint", Colors.White);
        return mat;
    }

    /// <summary>
    /// Create a material for SDF (signed distance field) fonts
    /// </summary>
    public static Material CreateSDFMaterial(IReadableTexture page)
    {
        Material mat = new Material(SdfShader);

        mat.SetUniform("mainTex", page);
        mat.SetUniform("thickness", 0.5f);
        mat.SetUniform("softness", 0.0f);
        mat.SetUniform("tint", Colors.White);
        return mat;
    }

    /// <summary>
    /// Create a material for MSDF (multichannel signed distance field) fonts
    /// </summary>
    public static Material CreateMSDFMaterial(IReadableTexture page)
    {
        Material mat = new Material(MsdfShader);

        //TODO meer pages
        mat.SetUniform("mainTex", page);
        mat.SetUniform("tint", Colors.White);
        return mat;
    }


}
