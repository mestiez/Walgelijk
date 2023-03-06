namespace Walgelijk.Onion;

public readonly struct OnionMaterial
{
    public static readonly Shader Shader = new Shader(
@"
#version 460

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec4 color;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = color;
   gl_Position = projection * view * model * vec4(position, 1.0);
}
",

@$"
#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform vec2 {SimpleDrawing.DrawingMaterialCreator.ScaleUniform};
uniform float {SimpleDrawing.DrawingMaterialCreator.RoundednessUniform} = 0;

uniform float {SimpleDrawing.DrawingMaterialCreator.OutlineWidthUniform} = 0;
uniform vec4 {SimpleDrawing.DrawingMaterialCreator.OutlineColourUniform} = vec4(0,0,0,0);

uniform sampler2D {SimpleDrawing.DrawingMaterialCreator.MainTexUniform};
uniform vec4 {SimpleDrawing.DrawingMaterialCreator.TintUniform} = vec4(1, 1, 1, 1);

// The MIT License
// Copyright (C) 2015 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), 
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions: 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// https://www.youtube.com/c/InigoQuilez
// https://iquilezles.org

float sdRoundBox( in vec2 p, in vec2 b, in float r ) 
{{
    vec2 q = abs(p - 0.5) * 2.0 * b -b + r;
    return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r;
}}

void main()
{{
    float corner = 1;
    float outline = 0;

    float clampedRoundness = clamp({SimpleDrawing.DrawingMaterialCreator.RoundednessUniform}, 0, min(scale.x / 2, scale.y / 2));
    float d = sdRoundBox(uv, scale, clampedRoundness * 2.0);

    corner = d < 0 ? 1 : 0;
    outline = d < -{SimpleDrawing.DrawingMaterialCreator.OutlineWidthUniform} ? 0 : 1;

    color = vertexColor * texture({SimpleDrawing.DrawingMaterialCreator.MainTexUniform}, uv) * mix(tint, {SimpleDrawing.DrawingMaterialCreator.OutlineColourUniform}, outline * {SimpleDrawing.DrawingMaterialCreator.OutlineColourUniform}.a);
    color.a *= corner;
}}
");

    public static Material CreateNew() => new(Shader);
}

public static class Onion
{
    public static readonly Layout.Layout Layout = new();
    public static readonly ControlTree Tree = new();
    public static readonly Navigator Navigator = new();
    public static readonly Input Input = new();
    public static readonly Configuration Configuration = new();
    public static Theme Theme = new();

    public static readonly Material ControlMaterial = OnionMaterial.CreateNew();

    /*TODO 
     * ClearEverything();
     * Windows!! draggables
     * scrollbars etc. (pseudo controls)
     * style
     *      style moet textures meer supporten, niet alleen kleuren 
     *      misschien zelfs iets anders dan quads
     *      uber shader voor alle controls
     * Sounds :)
     * Stack<Style> en dan bouw je voor elke control een final style misschien?
     * heel veel basic functies hier (label, button. etc.)
     * Animation system (IAnimation) deel van style? nee toch??? weet ik het 
     * navigation (arrows, tab)
     *      tab: cycle through all controls chronologically
     *      arrows: move from control to control based on position in space
    */
}

public class Theme
{
    public Appearance Background = new Color("#022525");
    public Appearance Foreground = new Color("#055555");
    public Color Text = new Color("#fcffff");
    public Color Accent = new Color("#de3a67");

    public Font Font = Walgelijk.Font.Default;
    public int FontSize = 12;

    public float Padding = 5;
    public float Rounding = 1;

    public Color FocusBoxColour = new Color("#3adeda");
    public float FocusBoxSize = 5;
    public float FocusBoxWidth = 4;
}

public class ThemeProperty<T> where T : notnull
{
    public readonly T Default;

    public ThemeProperty(in T @default)
    {
        Default = @default;
    }

    private readonly Stack<T> stack = new();

    public void Push(T val) => stack.Push(val);

    public T Pop() => stack.Pop();

    public T Get()
    {
        if (stack.TryPeek(out var val))
            return val;
        return Default;
    }

    public static implicit operator T(ThemeProperty<T> theme) => theme.Get();
}
