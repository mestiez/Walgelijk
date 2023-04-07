namespace Walgelijk.Onion.Controls;

public class OptionalControlState<T>
{
    public record State(T Value, bool IncomingChange);
    private static readonly Dictionary<int, State> states = new();

    public void SetValue(int identity, T value)
    {
        states.AddOrSet(identity, new State(value, true));
    }

    public T GetValue(int identity)
    {
        if (states.TryGetValue(identity, out var state))
            return state.Value;
        throw new Exception($"Identity {identity} asked for a state it does not have");
    }

    public void UpdateFor(int identity, ref T v)
    {
        if (states.TryGetValue(identity, out var state))
        {
            if (state.IncomingChange)
                v = state.Value;
            states[identity] = new(v, false);
        }
        else
            states.Add(identity, new(v, false));
    }

    public bool HasIncomingChange(int identity) => states.TryGetValue(identity, out var state) ? state.IncomingChange : false;

    public T this[int identity]
    {
        get => GetValue(identity);
        set => SetValue(identity, value);
    }
}
