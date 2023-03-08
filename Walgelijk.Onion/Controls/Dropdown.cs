using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Dropdown<T> : IControl
{
    public readonly IList<T> Values;
    public readonly bool DrawArrow;

    private static readonly Dictionary<int, CurrentState> currentStates = new();
    private static readonly Dictionary<Type, Array> enumValues = new();

    private record CurrentState
    {
        public int SelectedIndex;
        public Rect DropdownRect = default;
        public float TimeSinceFocus = float.MaxValue;

        public CurrentState(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }

    public Dropdown(IList<T> values, bool drawArrow)
    {
        Values = values;
        DrawArrow = drawArrow;
    }

    public static ControlState CreateForEnum<EnumType>(ref EnumType selected, bool arrow = true, int identity = 0, [CallerLineNumber] int site = 0) where EnumType : struct, Enum
    {
        EnumType[] arr;
        if (!enumValues.TryGetValue(typeof(EnumType), out var a))
        {
            arr = Enum.GetValues<EnumType>();
            enumValues.Add(typeof(EnumType), arr);
        }
        else
            arr = (a as EnumType[])!;

        int selectedIndex = Array.IndexOf(arr, selected);
        var result = Create(arr, ref selectedIndex, arrow, identity, site);
        selected = arr[selectedIndex];
        return result;
    }

    public static ControlState Create<ValueType>(IList<ValueType> values, ref int selectedIndex, bool arrow = true, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Dropdown<ValueType>).GetHashCode(), identity, site), new Dropdown<ValueType>(values, arrow));

        var dropdownRect = new Rect();
        if (currentStates.TryGetValue(instance.Identity, out var currentState))
            dropdownRect = currentState.DropdownRect;

        if (instance.IsActive)
        {
            float height = instance.Rects.ComputedGlobal.Height;

            Onion.Layout.Height(dropdownRect.Height);
            Onion.Layout.FitContainer(1, null);
            Onion.Layout.Offset(Onion.Theme.Padding, height + Onion.Theme.Padding);
            Onion.Tree.Start(instance.Identity + 38, new ScrollView());

            for (int i = 0; i < values.Count; i++)
            {
                Onion.Layout.Offset(0, i * height);
                Onion.Layout.Height(height);
                Onion.Layout.Width(instance.Rects.ComputedGlobal.Width - Onion.Theme.Padding * 2);
                Onion.Layout.CenterHorizontal();
                if (Button.Click(values[i]?.ToString() ?? "???", i + instance.Identity))
                {
                    currentStates[instance.Identity].SelectedIndex = i;
                }
            }

            Onion.Tree.End();
        }

        Onion.Tree.End();
        if (!currentStates.TryAdd(instance.Identity, new CurrentState(selectedIndex)))
            selectedIndex = currentStates[instance.Identity].SelectedIndex;
        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p) { }

    public void OnProcess(in ControlParams p)
    {
        var instance = p.Instance;
        var currentState = currentStates[instance.Identity];
        var o = instance.IsActive;
        ControlUtils.ProcessToggleLike(p);
        p.Instance.CaptureFlags |= CaptureFlags.Scroll;

        if (instance.IsActive != o)
            currentState.TimeSinceFocus = 0;
        else
            currentState.TimeSinceFocus += p.GameState.Time.DeltaTime;

        var computedGlobal = instance.Rects.ComputedGlobal;

        if (instance.HasScroll)
        {
            var v = p.Input.ScrollDelta.X + p.Input.ScrollDelta.Y;

            if (v < 0)
                currentState.SelectedIndex = (currentState.SelectedIndex + 1) % Values.Count;
            else
            {
                currentState.SelectedIndex--;
                if (currentState.SelectedIndex < 0)
                    currentState.SelectedIndex = Values.Count - 1;
            }
        }

        if (instance.IsActive)
        {
            var dropdownRect = new Rect(computedGlobal.MinX, computedGlobal.MaxY, computedGlobal.MaxX, computedGlobal.MaxY);
            var dropdownRectTargetHeight = instance.Rects.Rendered.Height * Values.Count + Onion.Theme.Padding * 2;

            dropdownRectTargetHeight *= Easings.Quad.Out(Utilities.Clamp(currentState.TimeSinceFocus / 0.1f));

            dropdownRectTargetHeight = MathF.Min(dropdownRectTargetHeight, ((Game.Main.Window.Height - Onion.Theme.Padding * 2) - computedGlobal.MaxY));

            dropdownRect.MaxY += dropdownRectTargetHeight;

            if (instance.Rects.DrawBounds.HasValue)
                instance.Rects.DrawBounds = instance.Rects.DrawBounds.Value.StretchToContain(dropdownRect);

            if (instance.Rects.Raycast.HasValue)
                instance.Rects.Raycast = instance.Rects.Raycast.Value.StretchToContain(dropdownRect);

            currentState.DropdownRect = dropdownRect;

            if (currentState.TimeSinceFocus > 0.1f && p.Instance.State.HasFlag(ControlState.Hover) && p.Input.MousePrimaryRelease)
                Onion.Navigator.FocusedControl = null;
        }
    }

    public void OnRender(in ControlParams p)
    {
        (ControlTree tree, Layout.Layout layout, Input input, GameState state, Node node, ControlInstance instance) = p;

        var currentState = currentStates[p.Node.Identity];

        var animation = node.Alive ?
            Utilities.Clamp(node.SecondsAlive / instance.AllowedDeadTime) :
            1 - Utilities.Clamp(node.SecondsDead / instance.AllowedDeadTime);
        animation = Easings.Cubic.InOut(animation);

        instance.Rects.Rendered = instance.Rects.Rendered.Scale(Utilities.Lerp(animation, 1, 0.6f));

        var fg = Onion.Theme.Foreground;
        Draw.Colour = fg.Color;
        Draw.Texture = fg.Texture;

        if (instance.IsHover)
        {
            IControl.SetCursor(DefaultCursor.Pointer);
            Draw.Colour = fg.Color.Brightness(1.2f);
        }
        if (instance.IsActive)
            Draw.Colour = fg.Color.Brightness(0.9f);

        Draw.Colour.A = (animation * animation * animation);
        var c = Draw.Colour;
        Draw.Quad(instance.Rects.Rendered, 0, Onion.Theme.Rounding);

        if (DrawArrow)
        {
            const float arrowSize = 8;
            var arrowPos = new Vector2(instance.Rects.Rendered.MaxX, (instance.Rects.Rendered.MinY + instance.Rects.Rendered.MaxY) / 2);
            arrowPos.X -= instance.Rects.Rendered.Height / 2;
            Draw.Colour = Onion.Theme.Accent;
            Draw.ResetTexture();
            Draw.TriangleIscoCentered(arrowPos, new Vector2(arrowSize), instance.IsActive ? 0 : 180);
        }

        if (instance.IsActive)
        {
            Draw.Colour = c;
            Draw.Texture = fg.Texture;
            Draw.Quad(currentState.DropdownRect, 0, Onion.Theme.Rounding);
        }

        if (animation > 0.5f)
        {
            Draw.ResetTexture();
            Draw.Font = Onion.Theme.Font;
            Draw.Colour = Onion.Theme.Text with { A = Draw.Colour.A };
            var selected = GetValue(currentState.SelectedIndex);
            Draw.Text(selected, instance.Rects.Rendered.GetCenter(), Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Middle, instance.Rects.Rendered.Width);
        }
    }

    private string GetValue(int selectedIndex)
    {
        return Values.Count > selectedIndex ? (Values[selectedIndex]?.ToString() ?? "NULL") : "OutOfBounds";
    }

    public void OnEnd(in ControlParams p)
    {
    }
}