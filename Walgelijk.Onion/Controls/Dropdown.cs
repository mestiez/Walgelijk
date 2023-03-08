using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion.Controls;

public readonly struct Dropdown<T> : IControl
{
    public readonly IList<T> Values;

    private static readonly Dictionary<int, CurrentState> currentStates = new();

    private record CurrentState
    {
        public int SelectedIndex;
        public Rect DropdownRect;

        public CurrentState(int selectedIndex, Rect dropdownRect)
        {
            SelectedIndex = selectedIndex;
            DropdownRect = dropdownRect;
        }
    }

    public Dropdown(IList<T> values)
    {
        Values = values;
    }

    public static ControlState Create<ValueType>(IList<ValueType> values, ref int selectedIndex, int identity = 0, [CallerLineNumber] int site = 0)
    {
        var (instance, node) = Onion.Tree.Start(IdGen.Hash(nameof(Dropdown<ValueType>).GetHashCode(), identity, site), new Dropdown<ValueType>(values));

        if (instance.State.HasFlag(ControlState.Active))
        {
            for (int i = 0; i < values.Count; i++)
            {
                //scrollview :)
                Onion.Layout.Offset(0, (i + 1) * 32);
                Onion.Layout.Height(32);
                Onion.Layout.FitContainer(1, null);
                Onion.Layout.CenterHorizontal();
                if (Button.Click(values[i]?.ToString() ?? "???", i + instance.Identity))
                {
                    currentStates[instance.Identity] = currentStates[instance.Identity] with
                    {
                        SelectedIndex = i
                    };
                }
            }
        }

        Onion.Tree.End();
        if (!currentStates.TryAdd(instance.Identity, new CurrentState(selectedIndex, default)))
            selectedIndex = currentStates[instance.Identity].SelectedIndex;
        return instance.State;
    }

    public void OnAdd(in ControlParams p) { }

    public void OnRemove(in ControlParams p) { }

    public void OnStart(in ControlParams p)
    {

    }

    public void OnProcess(in ControlParams p)
    {
        ControlUtils.ProcessToggleLike(p);

        var instance = p.Instance;
        var computedGlobal = p.Instance.Rects.ComputedGlobal;

        if (p.Instance.State.HasFlag(ControlState.Active))
        {
            var dropdownRect = new Rect(computedGlobal.MinX, computedGlobal.MinY, computedGlobal.MaxX, computedGlobal.MinY);
            dropdownRect.MaxY += instance.Rects.Rendered.Height * (Values.Count + 1);

            if (instance.Rects.DrawBounds.HasValue)
                instance.Rects.DrawBounds = instance.Rects.DrawBounds.Value.StretchToContain(dropdownRect);

            if (instance.Rects.Raycast.HasValue)
                instance.Rects.Raycast = instance.Rects.Raycast.Value.StretchToContain(dropdownRect);

            currentStates[instance.Identity] = currentStates[instance.Identity] with { DropdownRect = dropdownRect };

            //if (p.Instance.State.HasFlag(ControlState.Hover) && p.Input.MousePrimaryRelease)
            //    Onion.Navigator.FocusedControl = null;
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

        if (instance.State.HasFlag(ControlState.Hover))
        {
            IControl.SetCursor(DefaultCursor.Pointer);
            Draw.Colour = fg.Color.Brightness(1.2f);
        }
        if (instance.State.HasFlag(ControlState.Active))
            Draw.Colour = fg.Color.Brightness(0.9f);

        Draw.Colour.A = (animation * animation * animation);
        Draw.Quad(instance.Rects.Rendered, 0, Onion.Theme.Rounding);

        if (instance.State.HasFlag(ControlState.Active))
        {
            Draw.Colour = fg.Color;
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