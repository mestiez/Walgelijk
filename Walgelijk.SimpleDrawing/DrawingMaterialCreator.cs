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
    /// Circle morph factor
    /// </summary>
    public const string CircleUniform = "circle";

    /// <summary>
    /// Gets the default material with a white texture
    /// </summary>
    public static Material BasicMaterial => Cache.Load(Texture.White);

    public static string FragmentShader =
        @$"#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform vec2 {ScaleUniform} = vec2(1,1);
uniform float {RoundednessUniform} = 0;

uniform float {OutlineWidthUniform} = 0;
uniform float {CircleUniform} = 0;
uniform vec4 {OutlineColourUniform} = vec4(0,0,0,0);

uniform sampler2D {MainTexUniform};
uniform vec4 {TintUniform} = vec4(1, 1, 1, 1);
uniform int {ImageModeUniform} = 0;

// The MIT License
// Copyright (C) 2015 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// https://www.youtube.com/c/InigoQuilez
// https://iquilezles.org
// this function has been edited
float sdRoundBox(in vec2 p, in vec2 b, in float r ) 
{{
    vec2 q = abs(p - 0.5) * 2.0 * b - b + r;
    return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r;
}}
// this function has also been edited
float sdEllipse( vec2 p, vec2 ab )
{{
    p = abs(p - 0.5) * 2 * ab;
    vec2 q = ab*(p-ab);
    vec2 cs = normalize( (q.x<q.y) ? vec2(0.01,1) : vec2(1,0.01) );
    for( int i=0; i<5; i++ )
    {{
        vec2 u = ab*vec2( cs.x,cs.y);
        vec2 v = ab*vec2(-cs.y,cs.x);
        float a = dot(p-u,v);
        float c = dot(p-u,u) + dot(v,v);
        float b = sqrt(c*c-a*a);
        cs = vec2( cs.x*b-cs.y*a, cs.y*b+cs.x*a )/c;
    }}
    float d = length(p-ab*cs);
    return (dot(p/ab,p/ab)>1.0) ? d : -d;
}}

vec2 getSlicedUv(in vec2 p)
{{
    vec2 size = textureSize({MainTexUniform}, 0);
    vec2 ratio = {ScaleUniform} / size;
    vec2 edge = min(vec2(0.5), 0.5 / ratio);
    vec2 min = p * ratio;
    vec2 max = (p - 1.0) * ratio + 1.0;
    vec2 maxEdge = 1.0 - edge;
    return vec2(
        p.x > edge.x ? (p.x < maxEdge.x ? 0.5 : max.x) : min.x,
        p.y > edge.y ? (p.y < maxEdge.y ? 0.5 : max.y) : min.y
    );
}}

vec2 getTiledUv(in vec2 p)
{{
    vec2 size = textureSize({MainTexUniform}, 0);
    vec2 ratio = {ScaleUniform} / size;
    return p * ratio;
}}

void main()
{{
    float clampedRoundness = clamp({RoundednessUniform}, 0, min({ScaleUniform}.x, {ScaleUniform}.y) / 2);
    float d = mix(sdRoundBox(uv, {ScaleUniform}, clampedRoundness * 2.0),  sdEllipse(uv, {ScaleUniform}) * 2, {CircleUniform}) + {CircleUniform};

    float aa = abs(fwidth(d)) / 2;
    float shapeBoundary = smoothstep(-aa, aa, -d);
    float outline = {OutlineWidthUniform} > 0.1 ? smoothstep(-{OutlineWidthUniform} - aa, -{OutlineWidthUniform} + aa, d) : 0;

    vec2 wUv = ({ImageModeUniform} == 0) ? uv : ({ImageModeUniform} == 1 ? getSlicedUv(uv) : getTiledUv(uv));

    vec4 baseColor = texture({MainTexUniform}, wUv) * {TintUniform};
    color = vertexColor * mix(baseColor, {OutlineColourUniform}, outline * {OutlineColourUniform}.a);
    color.a *= shapeBoundary;
}}";

    public static readonly Shader DefaultShader = new(BuiltInShaders.WorldSpaceVertex, FragmentShader);

    /// <summary>
    /// Create a material for a texture
    /// </summary>
    public static Material Create(IReadableTexture tex)
    {
        var m = new Material(DefaultShader);
        m.DepthTested = false;
        m.SetUniform(MainTexUniform, tex);
        return m;
    }
}