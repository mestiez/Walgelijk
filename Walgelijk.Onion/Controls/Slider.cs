using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Slider : IControl
{
    private readonly Direction direction;
    private readonly MinMax<float> range;
    private readonly float step;

    public Slider(Direction direction, MinMax<float> range, float step)
    {
        this.direction = direction;
        this.range = range;
        this.step = step;
    }

    public enum Direction
    {
        Horizontal,
        Vertical
    }

    private static readonly OptionalControlState<float> states = new();

    public static ControlState Float(ref float value, Direction dir, MinMax<float> range, float step = 0, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Slider).GetHashCode(), (int)dir, identity, site), new Slider(dir, range, step));
        instance.RenderFocusBox = false;
        Onion.Tree.End();

        states.UpdateFor(instance.Identity, ref value);

        return instance.State;
    }

    public static ControlState Int(ref int value, Direction dir, MinMax<int> range, int step = 1, int identity = 0, [CallerLineNumber] int site = 0)
    {
        float vv = value;
        var rr = new MinMax<float>(range.Min, range.Max);
        var s = Float(ref vv, dir, rr, Math.Max(1, step), identity, site);
        if (s.HasFlag(ControlState.Active))
            value = (int)vv;
        return s;
    }

    public void OnAdd(in ControlParams p)
    {
    }

    public void OnStart(in ControlParams p)
    {
    }

    public void OnProcess(in ControlParams p)
    {
        ControlUtils.ProcessButtonLike(p);
        p.Instance.CaptureFlags |= CaptureFlags.Scroll;

        var v = 0f;

        float d;
        if (p.Input.CtrlHeld && p.Instance.HasScroll && (d = p.Input.ScrollDelta.LengthSquared()) > 0)
        {
            var s = Onion.Configuration.ScrollSensitivity * 0.01f;
            v = Utilities.Clamp(states[p.Identity] + d > 0 ? s : -s, range.Min, range.Max);
        }
        else if (p.Instance.IsActive)
        {
            var m = p.Input.MousePosition;
            var r = p.Instance.Rects.ComputedGlobal;
            switch (direction)
            {
                case Direction.Horizontal:
                    v = Utilities.Clamp(Utilities.MapRange(r.MinX, r.MaxX, range.Min, range.Max, m.X), range.Min, range.Max);
                    break;
                case Direction.Vertical:
                    v = Utilities.Clamp(Utilities.MapRange(r.MaxY, r.MinY, range.Min, range.Max, m.Y), range.Min, range.Max);
                    break;
            }
        }
        else return;

        states[p.Identity] = step > float.Epsilon ? Utilities.Snap(v, step) : v;
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

        var sliderRect = instance.Rects.Rendered;

        switch (direction)
        {
            case Direction.Horizontal:
                sliderRect.MaxX = Utilities.MapRange(range.Min, range.Max, sliderRect.MinX, sliderRect.MaxX, states[p.Identity]);
                break;
            case Direction.Vertical:
                sliderRect.MinY = Utilities.MapRange(range.Min, range.Max, sliderRect.MaxY, sliderRect.MinY, states[p.Identity]);
                break;
        }

        sliderRect.MaxX = MathF.Max(sliderRect.MaxX, sliderRect.MinX + Onion.Theme.Padding * 4);
        sliderRect.MinY = MathF.Min(sliderRect.MinY, sliderRect.MaxY - Onion.Theme.Padding * 4);

        Draw.Colour = Onion.Theme.Accent;
        anim.AnimateColour(ref Draw.Colour, t);
        Draw.Quad(sliderRect.Expand(-Onion.Theme.Padding), 0, Onion.Theme.Rounding);
    }

    public void OnEnd(in ControlParams p)
    {
    }

    public void OnRemove(in ControlParams p)
    {
    }
}
