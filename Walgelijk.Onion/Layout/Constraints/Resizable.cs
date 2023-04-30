using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct Resizable : IConstraint
{
    public static readonly OptionalControlState<ResizeState> ScaleStates = new(new ResizeState(Vector2.One, Axis.None));
    public readonly Vector2 MinSize, MaxSize;

    static Resizable()
    {
        Onion.OnClear.AddListener(ScaleStates.Clear);
    }

    public Resizable(Vector2 minSize, Vector2 maxSize)
    {
        MinSize = minSize;
        MaxSize = maxSize;
    }

    public struct ResizeState
    {
        public Vector2 Scale;
        public Axis Axis;

        public ResizeState(Vector2 scale, Axis axis)
        {
            Scale = scale;
            Axis = axis;
        }
    }

    [Flags]
    public enum Axis : byte
    {
        None = 0b_00,
        Horizontal = 0b_01,
        Vertical = 0b_10,
        Both = 0b_11,
    }

    public void Apply(in ControlParams p)
    {
        const float edgeDragSize = 8;
        var state = ScaleStates[p.Identity];

        var oldRect = p.Instance.Rects.Intermediate;
        p.Instance.Rects.Intermediate.Width *= state.Scale.X;
        p.Instance.Rects.Intermediate.Height *= state.Scale.Y;
        p.Instance.Rects.Raycast = p.Instance.Rects.ComputedGlobal;
        var rect = p.Instance.Rects.Intermediate;

        p.Instance.CaptureFlags |= CaptureFlags.Hover;

        var dist = SDF.Rectangle(p.Input.MousePosition, rect.BottomLeft, rect.GetSize());
        var hoverDraggable = dist > -edgeDragSize && dist < 0 && p.Instance.IsHover;

        if (hoverDraggable)
        {
            var axis = Axis.None;

            if (!p.Instance.IsTriggered)
            {
                if (p.Input.MousePosition.X > rect.MaxX - edgeDragSize)
                    axis |= Axis.Horizontal;
                if (p.Input.MousePosition.Y > rect.MaxY - edgeDragSize)
                    axis |= Axis.Vertical;

                SetCursor(axis);
            }

            if (p.Input.MousePrimaryPressed)
            {
                state.Axis = axis;
                Onion.Navigator.FocusedControl = p.Instance.Identity;
                Onion.Navigator.TriggeredControl ??= p.Instance.Identity;
            }
        }

        if (p.Instance.IsTriggered)
        {
            var newTarget = p.Instance.Rects.Intermediate.GetSize() + p.Input.MouseDelta;
            newTarget.X = Utilities.Clamp(newTarget.X, MinSize.X, MaxSize.X);
            newTarget.Y = Utilities.Clamp(newTarget.Y, MinSize.Y, MaxSize.Y);

            if (state.Axis.HasFlag(Axis.Horizontal))
                state.Scale.X = newTarget.X / oldRect.Width;
            if (state.Axis.HasFlag(Axis.Vertical))
                state.Scale.Y = newTarget.Y / oldRect.Height;

            SetCursor(state.Axis);

            if (p.Input.MousePrimaryRelease)
                Onion.Navigator.TriggeredControl = null;

        }

        ScaleStates[p.Identity] = state;
    }

    private static void SetCursor(Axis axis)
    {
        if (axis == Axis.Vertical)
            IControl.SetCursor(DefaultCursor.VerticalResize);
        else if (axis == Axis.Horizontal)
            IControl.SetCursor(DefaultCursor.HorizontalResize);
        else if (axis == Axis.Both)
            IControl.SetCursor(DefaultCursor.Crosshair);
    }
}
