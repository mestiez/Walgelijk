namespace Walgelijk.Onion.Decorators;

public class DecoratorQueue
{
    public const int MaxDecoratorsPerControl = 8;

    /// <summary>
    /// Default decorators to apply
    /// </summary>
    public List<IDecorator>? Default = null;

    internal readonly List<IDecorator> Queue = new();
    internal bool ForceNoDecorators = false;
    private int keepForNextStack = 0; // the decorator queue should be retained for x amount of cycles

    public void Add<T>(in T d) where T : IDecorator
    {
        if (Queue.Count >= MaxDecoratorsPerControl)
            throw new Exception($"A control may not have more than {MaxDecoratorsPerControl} decorators");
        Queue.Add(d);
    }

    public void Clear()
    {
        keepForNextStack = 0;
        Queue.Clear();
    }

    public void DoNotDecorate() => ForceNoDecorators = true;

    public void KeepForNextTime() => keepForNextStack++;

    public void Process(ControlInstance inst)
    {
        inst.Decorators.Clear();
        bool f = ForceNoDecorators;

        if (keepForNextStack <= 0)
            ForceNoDecorators = false;

        if (!f)
        {
            if (Queue.Count == 0)
            {
                if (Default != null)
                    for (int i = 0; i < Default.Count; i++)
                        inst.Decorators.Add(Default[i]);
            }
            else
            {
                //while (Queue.TryDequeue(out var next))
                foreach (var next in Queue)
                    inst.Decorators.Add(next);

                if (keepForNextStack <= 0)
                    Queue.Clear();

                keepForNextStack = Math.Max(0, keepForNextStack - 1);
            }
        }

        
    }

    public DecoratorQueue Tooltip(in string message)
    {
        Add(new Tooltip(message));
        return this;
    }
}
