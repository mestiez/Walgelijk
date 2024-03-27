using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct ColourPicker : IControl
{
    public const int BottomBarHeight = 64;
    public const int SliderHeight = BottomBarHeight / 3;
    public const int HueSliderWidth = 32;
    public const int AlphaSliderHeight = 32;

    public readonly bool EditableAlpha;

    public ColourPicker(bool editableAlpha)
    {
        EditableAlpha = editableAlpha;
    }

    private static readonly Texture rainbowTexture;
    private static readonly Texture checkboardTexture;
    private static readonly OptionalControlState<ColourPickerState> states = new();

    private static readonly Material hsBox = new Material(new Shader(Shader.Default.VertexShader,
@"#version 330 core

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform vec4 tint;
uniform float u_hue;
uniform sampler2D mainTex;

float fff(float n, float h, float s, float v) 
{
    float k = mod(n + h / 60.0, 6.0);
    return v - v * s * max(min(k, min(4.0 - k, 1.0)), 0.0);
}

vec3 fromHsv(float h, float s, float v) 
{
    h *= 360.0;
    return vec3(fff(5, h, s, v), fff(3, h, s, v), fff(1, h, s, v));
}

float diffc(float c, float v, float diff) 
{
    return (v - c) / 6.0 / diff + 0.5;
}

vec3 toHsv(in vec3 rgb) 
{
    float rabs, gabs, babs, rr, gg, bb, diff;
    float h = 0.0;
    float s = 0.0;
    float v = 0.0;

    rabs = rgb.r;
    gabs = rgb.g;
    babs = rgb.b;
    v = max(rabs, max(gabs, babs));
    diff = v - min(rabs, min(gabs, babs));

    if (diff == 0.0) {
        h = s = 0.0;
    } else {
        s = diff / v;
        rr = diffc(rabs, v, diff);
        gg = diffc(gabs, v, diff);
        bb = diffc(babs, v, diff);

        if (rabs == v) {
            h = bb - gg;
        } else if (gabs == v) {
            h = (1.0 / 3.0) + rr - bb;
        } else if (babs == v) {
            h = (2.0 / 3.0) + gg - rr;
        }
        if (h < 0.0) {
            h += 1.0;
        } else if (h > 1.0) {
            h -= 1.0;
        }
    }

    return vec3(h,s,v);
}

void main()
{
    float h = u_hue;
    float s = uv.x;
    float v = uv.y;

    color = vec4(fromHsv(h, s, v), 1);
    color.a = tint.a;
    //color = tint * vertexColor * texture(mainTex, uv);
}"
));

    private record ColourPickerState(Color Color, float SelectedHue);

    public Rect GetSVRect(in ControlParams p)
    {
        var r = p.Instance.Rects.ComputedGlobal.Expand(-p.Theme.Padding);
        r.MaxY -= BottomBarHeight * Onion.GlobalScale + p.Theme.Padding;
        r.MaxX -= HueSliderWidth * Onion.GlobalScale;
        if (EditableAlpha)
            r.MaxY -= AlphaSliderHeight * Onion.GlobalScale;
        return r;
    }

    public Rect GetHueRect(in ControlParams p)
    {
        var r = p.Instance.Rects.ComputedGlobal.Expand(-p.Theme.Padding);
        r.MaxY -= BottomBarHeight * Onion.GlobalScale + p.Theme.Padding;
        r.MinX = r.MaxX - HueSliderWidth * Onion.GlobalScale;
        if (EditableAlpha)
            r.MaxY -= AlphaSliderHeight * Onion.GlobalScale;
        return r;
    }

    static ColourPicker()
    {
        const int res = 128;
        rainbowTexture = new Texture(1, res, false, false);
        rainbowTexture.FilterMode = FilterMode.Linear;
        rainbowTexture.WrapMode = WrapMode.Mirror;

        for (int y = 0; y < res; y++)
        {
            var c = Color.FromHsv(1 - (float)y / res, 1, 1);
            rainbowTexture.SetPixel(0, y, c);
        }

        rainbowTexture.ForceUpdate();
        rainbowTexture.DisposeLocalCopyAfterUpload = true;

        checkboardTexture = TexGen.Checkerboard(100, 32, 4, Colors.White, Colors.Gray);
    }

    public static Color GetColor(Vector2 pos, float hue)
    {
        return Color.FromHsv(hue, pos.X, 1 - pos.Y);
    }

    public static Vector2 GetPosition(Color col)
    {
        col.GetHsv(out _, out float x, out float y);
        return new Vector2(x, 1 - y);
    }

    public static ControlState Create(ref Color value, bool editableAlpha = false, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Create(nameof(ColourPicker).GetHashCode(), identity, site), new ColourPicker(editableAlpha));
        instance.RenderFocusBox = false;

        float inputWidth = 100;

        Onion.Layout.Width(inputWidth).Height(32).StickRight().StickBottom();
        if (InputBox.String(ref instance.Name, new TextBoxOptions(), instance.Identity))
        {
            try
            {
                value = new Color(instance.Name);
            }
            catch (Exception)
            {
            }
        }

        var sliderWidth = (instance.Rects.ComputedGlobal.Width / Onion.GlobalScale - inputWidth - instance.Theme.Padding * 3 / Onion.GlobalScale);
        bool sliderInput = false;

        Onion.Layout.Width(sliderWidth).Height(SliderHeight).StickLeft().StickBottom();
        Onion.Theme.Accent(Colors.Blue).Once();
        sliderInput |= Slider.Float(ref value.B, Direction.Horizontal, (0, 1), 0.025f, null, instance.Identity);

        Onion.Layout.Width(sliderWidth).Height(SliderHeight).StickLeft().StickBottom().Move(0, -SliderHeight);
        Onion.Theme.Accent(Colors.Green).Once();
        sliderInput |= Slider.Float(ref value.G, Direction.Horizontal, (0, 1), 0.025f, null, instance.Identity);

        Onion.Layout.Width(sliderWidth).Height(SliderHeight).StickLeft().StickBottom().Move(0, -SliderHeight * 2);
        Onion.Theme.Accent(Colors.Red).Once();
        sliderInput |= Slider.Float(ref value.R, Direction.Horizontal, (0, 1), 0.025f, null, instance.Identity);

        if (editableAlpha)
        {
            Onion.Layout.FitWidth().Height(AlphaSliderHeight - instance.Theme.Padding).StickLeft().StickBottom().Move(0, -SliderHeight * 2 - AlphaSliderHeight + instance.Theme.Padding);
            sliderInput |= Slider.Float(ref value.A, Direction.Horizontal, (0, 1), 0.025f, "Alpha", instance.Identity);
        }

        Onion.Tree.End();

        if (sliderInput)
            instance.Name = value.ToHexCode();

        value.GetHsv(out var hue, out _, out _);
        var vv = new ColourPickerState(value, hue);

        if (states.UpdateFor(instance.Identity, ref vv))
        {
            instance.Name = value.ToHexCode();
            value = vv.Color;
        }

        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p) { }

    public void OnProcess(in ControlParams p)
    {
        p.Instance.CaptureFlags = CaptureFlags.Hover;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        p.Instance.Rects.DrawBounds = p.Instance.Rects.ComputedGlobal;

        ControlUtils.ProcessButtonLike(p);
        var state = states[p.Identity];

        if (p.Instance.IsActive)
        {
            var svRect = GetSVRect(p);
            var hueRect = GetHueRect(p);
            if (svRect.ContainsPoint(p.Input.MousePosition))
            {
                var v = svRect.ClosestPoint(p.Input.MousePosition);
                v.X = Utilities.MapRange(svRect.MinX, svRect.MaxX, 0, 1, v.X);
                v.Y = Utilities.MapRange(svRect.MinY, svRect.MaxY, 0, 1, v.Y);
                var col = GetColor(v, state.SelectedHue).WithAlpha(state.Color.A);
                states[p.Identity] = state with { Color = col };
            }
            else if (hueRect.ContainsPoint(p.Input.MousePosition))
            {
                var vv = hueRect.ClosestPoint(p.Input.MousePosition);
                float hue = Utilities.MapRange(hueRect.MinY, hueRect.MaxY, 0, 1, vv.Y);
                state.Color.GetHsv(out _, out var s, out var v);
                states[p.Identity] = state with { Color = Color.FromHsv(hue, s, v).WithAlpha(state.Color.A), SelectedHue = hue };
            }
        }
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.LayoutQueue layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        float inputWidth = 100 * Onion.GlobalScale;
        float inputHeight = 32 * Onion.GlobalScale;

        var t = node.GetAnimationTime();
        var anim = instance.Animations;
        var pickedColour = states.GetValue(p.Identity);
        var svRect = GetSVRect(p);
        var hueRect = GetHueRect(p);
        var value = states[p.Identity];

        var fg = p.Theme.Foreground[ControlState.None];
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;
        Draw.ImageMode = fg.ImageMode;

        anim.AnimateRect(ref instance.Rects.Rendered, t);
        anim.AnimateRect(ref svRect, t);
        anim.AnimateRect(ref hueRect, t);
        anim.AnimateColour(ref Draw.Colour, t);

        Draw.Quad(instance.Rects.Rendered, 0, p.Theme.Rounding);

        var previewBox = new Rect(0, 0, inputWidth, inputHeight - p.Theme.Padding).
            Translate(p.Theme.Padding, p.Theme.Padding + 2).
            Translate(instance.Rects.Rendered.BottomLeft).
            Translate(instance.Rects.Rendered.Width - inputWidth - p.Theme.Padding * 2, instance.Rects.Rendered.Height - inputHeight * 2 - p.Theme.Padding * 2);

        if (EditableAlpha)
        {
            Draw.Texture = checkboardTexture;
            Draw.Colour = Colors.White;
            anim.AnimateColour(ref Draw.Colour, t);
            Draw.Quad(previewBox, 0, p.Theme.Rounding);
        }

        Draw.ResetTexture();
        Draw.ImageMode = default;
        Draw.Colour = value.Color;
        if (!EditableAlpha)
            Draw.Colour.A = 1;
        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Colour = pickedColour.Color.WithAlpha(Draw.Colour.A);
        Draw.Quad(previewBox, 0, p.Theme.Rounding);

        Draw.ResetTexture();
        Draw.Colour = Colors.White;
        anim.AnimateColour(ref Draw.Colour, t);
        //Draw.Colour = Colors.White.WithAlpha(Draw.Colour.A);
        Draw.Material = hsBox;
        hsBox.SetUniform("u_hue", pickedColour.SelectedHue);
        Draw.Quad(svRect);

        Draw.ResetMaterial();
        Draw.Texture = rainbowTexture;
        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Colour = Colors.White.WithAlpha(Draw.Colour.A);
        Draw.Quad(hueRect);
        Draw.ResetTexture();

        Draw.Colour = Vector4.One - value.Color;
        Draw.Colour.A = 1;
        anim.AnimateColour(ref Draw.Colour, t);

        const float triangleSize = 8;
        var hueBarY = Utilities.MapRange(0, 1, hueRect.MinY, hueRect.MaxY, value.SelectedHue);
        Draw.TriangleIscoCentered(new Vector2(hueRect.MinX + triangleSize / 2, hueBarY), new Vector2(triangleSize), -90);
        Draw.TriangleIscoCentered(new Vector2(hueRect.MaxX - triangleSize / 2, hueBarY), new Vector2(triangleSize), 90);
        var colourPos = GetPosition(value.Color);
        colourPos.X = Utilities.MapRange(0, 1, svRect.MinX, svRect.MaxX, colourPos.X);
        colourPos.Y = Utilities.MapRange(0, 1, svRect.MinY, svRect.MaxY, colourPos.Y);

        Draw.Circle(colourPos, new Vector2(4));
    }

    public void OnEnd(in ControlParams p) { }
}
