namespace Walgelijk.Onion;

public struct ThemeProperty<T> where T : struct
{
    public T Default;
    public T? Hover;
    public T? Active;
    public T? Triggered;

    public ThemeProperty(T @default, T? hover = null, T? active = null, T? triggered = null) : this()
    {
        Default = @default;
        Hover = hover;
        Active = active;
        Triggered = triggered;
    }

    public readonly T Get(ControlState state)
    {
        if (Triggered != null && state.HasFlag(ControlState.Triggered))
            return Triggered.Value;

        if (Active != null && state.HasFlag(ControlState.Active))
            return Active.Value;

        if (Hover != null && state.HasFlag(ControlState.Hover))
            return Hover.Value;

        return Default;
    }

    public void Set(ControlState state, T value)
    {
        if (state.HasFlag(ControlState.Triggered))
            Triggered = value;
        else if (state.HasFlag(ControlState.Active))
            Active = value;
        else if (state.HasFlag(ControlState.Hover))
            Hover = value;

        Default = value;
    }

    //public static implicit operator T(ThemeProperty<T> theme) => theme.Get(ControlState.None);
    public static implicit operator ThemeProperty<T>(T val) => new(val);

    public T this[ControlState state]
    {
        get => Get(state);
        set => Set(state, value);
    }
}

//public class ThemeProperty<T> where T : notnull
//{
//    public readonly T Default;

//    public ThemeProperty(in T @default)
//    {
//        Default = @default;
//    }

//    private readonly Stack<T> stack = new();

//    public void Push(T val) => stack.Push(val);

//    public T Pop() => stack.Pop();

//    public T Get()
//    {
//        if (stack.TryPeek(out var val))
//            return val;
//        return Default;
//    }

//    public static implicit operator T(ThemeProperty<T> theme) => theme.Get();
//}
