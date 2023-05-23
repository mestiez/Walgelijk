using System;
using System.Linq;

namespace Walgelijk;

/// <summary>
/// Utility struct that provides static text material creation functions
/// </summary>
public struct FontMaterial
{
    public static readonly Shader BitmapShader = new Shader(ShaderDefaults.WorldSpaceVertex, BitmapFragmentShader);
    public static readonly Shader SdfShader = new Shader(ShaderDefaults.WorldSpaceVertex, SdfFragmentShader);
    public static readonly Shader MsdfShader = new Shader(ShaderDefaults.WorldSpaceVertex, MsdfFragmentShader);

    /// <summary>
    /// Create a material for a given font
    /// </summary>
    /// <param name="font"></param>
    /// <returns></returns>
    public static Material CreateFor(Font font)
    {
        switch (font.Rendering)
        {
            case FontRendering.Bitmap:
                return CreateSDFMaterial(font.Page);
            case FontRendering.SDF:
                return CreateSDFMaterial(font.Page);
            case FontRendering.MSDF:
                return CreateMSDFMaterial(font.Page);
        }
        throw new ArgumentException("Invalid FontRendering value: only Bitmap, SDF, and MSDF are supported by this function");
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
        Material mat = new Material(SdfShader);

        //TODO meer pages
        mat.SetUniform("mainTex", page);
        mat.SetUniform("thickness", 0.5f);
        mat.SetUniform("softness", 0.0f);
        mat.SetUniform("tint", Colors.White);
        return mat;
    }

    public const string BitmapFragmentShader =
@"#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;
uniform vec4 tint;

void main()
{
    vec4 tex = texture(mainTex, uv);
    color = vertexColor * tint;
    color.a = min(tex.r, min(tex.g, min(tex.b, min(tex.a, color.a))));
}
";

    public const string SdfFragmentShader =
@"#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;
uniform float thickness;
uniform float softness;
uniform vec4 tint;

void main()
{
    vec4 tex = texture(mainTex, uv);
    float t = clamp(1-thickness, 0, 1);

    float delta = fwidth(tex.a) / 2 + softness;
    tex.a = smoothstep(t - delta, t + delta , tex.a);
    color = vec4(vertexColor.rgb, vertexColor.a * tex.a) * tint;
}
";

    public const string MsdfFragmentShader =
@"#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;
uniform float softness;

float msdf(vec3 rgb) {
    float dist = median(rgb.r, rgb.g, rgb.b);
    return smoothstep(0.5 - softness/2.0, 0.5 + softness/2.0, dist);
}

void main() {
    vec3 msd = texture(mainTex, uv).rgb;
    float opacity = msdf(msd) * smoothstep(0.0, 1.0, fwidth(msdf(msd)));
    color = mix(bgColor, fgColor, opacity);
}
";
}
