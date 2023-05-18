namespace Walgelijk.Onion;

/// <summary>
/// The theme stack allows global and individual themes
/// </summary>
public class ThemeStack
{
    // Walgelijk.Onion.SourceGenerator generates a bunch of extension functions for this class

    /// <summary>
    /// The final fallback theme. If the stack is empty and <see cref="Next"/> has no value, this theme is used.
    /// </summary>
    public Theme Base = new();

    private bool onlyApplyToNextControl = false;
    private readonly Stack<Theme> stack = new();

    /// <summary>
    /// The theme to edit for the current context
    /// </summary>
    public Theme? Next;

    /// <summary>
    /// Clear the stack and clear all current changes
    /// </summary>
    public void Reset()
    {
        Next = null;
        stack.Clear();
    }

    /// <summary>
    /// Revert changes after push
    /// </summary>
    public void Pop()
    {
        Next = null;
        stack.Pop();
    }

    /// <summary>
    /// Try and get the most recent changes, excluding <see cref="Next"/>
    /// </summary>
    public Theme? Peek()
    {
        if (stack.TryPeek(out var p))
            return p;
        return null;
    }

    /// <summary>
    /// Make the current theme identical to the given theme
    /// </summary>
    public ThemeStack SetAll(in Theme theme)
    {
        Next = theme;
        return this;
    }

    /// <summary>
    /// Only apply the current changes to the next control. If this isn't called, the changes will persist until the frame ends or until <see cref="Pop"/> or <see cref="Reset"/> is called.
    /// </summary>
    public void Once() => onlyApplyToNextControl = true;

    internal void ApplyTo(ControlInstance inst)
    {
        if (Next.HasValue)
            stack.Push(Next.Value);

        inst.Theme = stack.Count > 0 ? stack.Peek() : Base;

        if (onlyApplyToNextControl)
            Pop();
        
        onlyApplyToNextControl = false;
    }
}