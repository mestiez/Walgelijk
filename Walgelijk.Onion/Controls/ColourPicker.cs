using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct ColourPicker : IControl
{
    private static readonly Texture rainbowTexture;
    private static readonly OptionalControlState<ColourPickerState> states = new();

    private record ColourPickerState(Color Color, float SelectedHue);

    static ColourPicker()
    {
        const int res = 128;
        rainbowTexture = new Texture(res, 1, false, false);
        rainbowTexture.FilterMode = FilterMode.Linear;

        for (int x = 0; x < res; x++)
        {
            var c = Color.FromHsv((float)x / res, 1, 1);
            rainbowTexture.SetPixel(x, 0, c);
        }

        rainbowTexture.ForceUpdate();
        rainbowTexture.DisposeLocalCopyAfterUpload = true;
    }

    public static Color GetColourAt(Vector2 pos, float hue)
    {
        return Color.FromHsv(hue, pos.X, 1 - pos.Y);
    }

    public static Vector2 GetPositionFor(Color col)
    {
        col.GetHsv(out _, out float x, out float y);
        return new Vector2(x, 1 - y);
    }

    public static ControlState Create(ref Color value, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(ColourPicker).GetHashCode(), identity, site), new ColourPicker());
        instance.RenderFocusBox = false;
        Onion.Tree.End();

        if (states.TryGetState(instance.Identity, out var state))
        {
            if (states.HasIncomingChange(instance.Identity))
                value = state.Color;
            else
                states[instance.Identity] = state with { Color = value };
        }
        else
        {
            value.GetHsv(out var hue, out _, out _);
            states[instance.Identity] = new ColourPickerState(value, hue);
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
            var v = p.Instance.Rects.Rendered.ClosestPoint(p.Input.MousePosition);
            v.X = Utilities.MapRange(p.Instance.Rects.Rendered.MinX, p.Instance.Rects.Rendered.MaxX, 0, 1, v.X);
            v.Y = Utilities.MapRange(p.Instance.Rects.Rendered.MinY, p.Instance.Rects.Rendered.MaxY, 0, 1, v.Y);
            var col = GetColourAt(v, state.SelectedHue);
            states[p.Identity] = state with { Color = col };
        }
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.Layout layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var t = node.GetAnimationTime();
        var anim = instance.Animations;

        var fg = Onion.Theme.Foreground;
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;

        anim.AnimateRect(ref instance.Rects.Rendered, t);

        if (instance.IsHover)
            Draw.Colour = fg.Color.Brightness(1.2f);
        if (instance.IsActive)
            Draw.Colour = fg.Color.Brightness(0.9f);

        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Quad(instance.Rects.Rendered, 0, Onion.Theme.Rounding);

        Draw.Colour = Color.White;// states[p.Identity];
        Draw.Texture = rainbowTexture;
        Draw.Quad(instance.Rects.Rendered.Expand(-Onion.Theme.Padding));
        Draw.ResetTexture();

        Draw.Colour = Colors.White;
        Draw.Colour.A = 1;
        var colourPos = GetPositionFor(states[p.Identity].Color);

        colourPos.X = Utilities.MapRange(0, 1, instance.Rects.Rendered.MinX, instance.Rects.Rendered.MaxX, colourPos.X);
        colourPos.Y = Utilities.MapRange(0, 1, instance.Rects.Rendered.MinY, instance.Rects.Rendered.MaxY, colourPos.Y);

        Draw.Circle(colourPos, new Vector2(4));
    }

    public void OnEnd(in ControlParams p) { }
}
