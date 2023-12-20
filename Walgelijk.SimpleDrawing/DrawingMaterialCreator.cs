namespace Walgelijk.SimpleDrawing;

/// <summary>
/// Responsible for creating materials that support textures, outline, and roundness
/// </summary>
public static class DrawingMaterialCreator
{
    /// <summary>
    /// Reference to the material cache
    /// </summary>
    public static readonly DrawingMaterialCache Cache = new();

    /// <summary>
    /// Main texture uniform
    /// </summary>
    public const string MainTexUniform = "mainTex";
    /// <summary>
    /// Actual transformation scale uniform
    /// </summary>
    public const string ScaleUniform = "scale";
    /// <summary>
    /// The roundness uniform
    /// </summary>
    public const string RoundednessUniform = "roundedness";
    /// <summary>
    /// Outline width uniform
    /// </summary>
    public const string OutlineWidthUniform = "outlineWidth";
    /// <summary>
    /// Outline colour uniform
    /// </summary>
    public const string OutlineColourUniform = "outlineColour";
    /// <summary>
    /// Colour uniform
    /// </summary>
    public const string TintUniform = "tint";
    /// <summary>
    /// Image mode uniform
    /// </summary>
    public const string ImageModeUniform = "imageMode";

    /// <summary>
    /// Gets the default material with a white texture
    /// </summary>
    public static Material BasicMaterial => Cache.Load(Texture.White);

    public const string FragmentShader =
        @$"#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform vec2 {ScaleUniform};
uniform float {RoundednessUniform} = 0;

uniform float {OutlineWidthUniform} = 0;
uniform vec4 {OutlineColourUniform} = vec4(0,0,0,0);

uniform sampler2D {MainTexUniform};
uniform vec4 {TintUniform} = vec4(1, 1, 1, 1);
uniform int {ImageModeUniform} = 0;

// The MIT License
// Copyright (C) 2015 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// https://www.youtube.com/c/InigoQuilez
// https://iquilezles.org

float sdRoundBox(in vec2 p, in vec2 b, in float r ) 
{{
    vec2 q = abs(p - 0.5) * 2.0 * b -b + r;
    return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r;
}}

vec2 getTiledUv(in vec2 p)
{{
    vec2 size = textureSize({MainTexUniform}, 0);
    vec2 ratio = {ScaleUniform} / size;
    vec2 edge = min(vec2(0.5, 0.5), 0.5 / ratio);
    vec2 min = p * ratio;
    vec2 max = (p - 1) * ratio + 1;
    vec2 maxEdge = 1.0 - edge;
    return vec2(
        p.x > edge.x ? (p.x < maxEdge.x ? 0.5 : max.x) : min.x,
        p.y > edge.y ? (p.y < maxEdge.y ? 0.5 : max.y) : min.y
    );
}}

void main()
{{
    float corner = 1;
    float outline = 0;

    float clampedRoundness = clamp({RoundednessUniform}, 0, min({ScaleUniform}.x / 2, {ScaleUniform}.y / 2));
    float d = sdRoundBox(uv, {ScaleUniform}, clampedRoundness * 2.0);

    corner = d < 0 ? 1 : 0;
    outline = {OutlineWidthUniform} < 1 ? 0 : (ceil(d + fwidth(uv.x) + 0.05) < -{OutlineWidthUniform} ? 0 : 1);

    vec2 wUv = getTiledUv(uv);
    
    wUv.x = {ImageModeUniform} == 0 ? uv.x : wUv.x;
    wUv.y = {ImageModeUniform} == 0 ? uv.y : wUv.y;

    color = vertexColor * texture({MainTexUniform}, wUv) * mix({TintUniform}, {OutlineColourUniform}, outline * {OutlineColourUniform}.a);
    color.a *= corner;
}}";

    public static readonly Shader DefaultShader = new(ShaderDefaults.WorldSpaceVertex, FragmentShader);

    /// <summary>
    /// Create a material for a texture
    /// </summary>
    public static Material Create(IReadableTexture tex)
    {
        var m = new Material(DefaultShader);
        m.SetUniform(MainTexUniform, tex);
        return m;
    }
}