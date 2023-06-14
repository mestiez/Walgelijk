namespace Walgelijk.Onion;

public struct StateDependent<T> where T : struct
{
    public T Default;
    public T? Hover;
    public T? Active;
    public T? Triggered;

    public StateDependent(T @default, T? hover = null, T? active = null, T? triggered = null) : this()
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
    public static implicit operator StateDependent<T>(T val) => new(val);

    public T this[ControlState state]
    {
        get => Get(state);
        set => Set(state, value);
    }
}
