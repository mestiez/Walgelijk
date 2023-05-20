namespace Walgelijk.Onion.Decorators;

public class DecoratorQueue
{
    public const int MaxDecoratorsPerControl = 8;

    /// <summary>
    /// Default decorators to apply
    /// </summary>
    public List<IDecorator>? Default = null;

    internal readonly Queue<IDecorator> Queue = new();
    internal bool ForceNoDecorators = false;

    public void Add<T>(in T d) where T : IDecorator
    {
        if (Queue.Count >= MaxDecoratorsPerControl)
            throw new Exception($"A control may not have more than {MaxDecoratorsPerControl} decorators");
        Queue.Enqueue(d);
    }

    public void Clear() => Queue.Clear();

    public void DoNotDecorate() => ForceNoDecorators = true;

    public void Process(ControlInstance inst)
    {
        inst.Decorators.Clear();

        if (!ForceNoDecorators)
        {
            if (Queue.Count == 0)
            {
                if (Default != null)
                    for (int i = 0; i < Default.Count; i++)
                        inst.Decorators.Add(Default[i]);
            }
            else
            {
                while (Queue.TryDequeue(out var next))
                    inst.Decorators.Add(next);
            }
        }

        ForceNoDecorators = false;
    }

    public DecoratorQueue Tooltip(in string message)
    {
        Add(new Tooltip(message));
        return this;
    }
}
