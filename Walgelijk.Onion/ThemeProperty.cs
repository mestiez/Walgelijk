namespace Walgelijk.Onion;

public class ThemeProperty<T>
{
    public T Base;

    public ThemeProperty(T @default)
    {
        Base = @default;
    }

    public readonly Stack<T> Stack = new();

    public T Pop()
    {
        if (Stack.TryPop(out var val))
            return val;
        return Base;
    }

    public void Push(T val) => Stack.Push(val);

    public static implicit operator ThemeProperty<T>(T val) => new(val);
    public static implicit operator T(ThemeProperty<T> val) => val.Pop();
}
