namespace Walgelijk.Onion.Decorators;

public class DecoratorQueue
{
    /// <summary>
    /// Maximum number of decorators allowed per control.
    /// </summary>
    public const int MaxDecoratorsPerControl = 8;

    /// <summary>
    /// Default decorators to apply
    /// </summary>
    public List<IDecorator>? Default = null;

    internal readonly List<IDecorator> Queue = new();
    internal bool ForceNoDecorators = false;
    private int keepForNextStack = 0; // the decorator queue should be retained for x amount of cycles

    /// <summary>
    /// Adds a decorator to the queue.
    /// </summary>
    public void Add<T>(in T d) where T : IDecorator
    {
        if (Queue.Count >= MaxDecoratorsPerControl)
            throw new Exception($"A control may not have more than {MaxDecoratorsPerControl} decorators");
        Queue.Add(d);
    }

    /// <summary>
    /// Clears the decorator queue.
    /// </summary>
    public void Clear()
    {
        keepForNextStack = 0;
        Queue.Clear();
    }

    /// <summary>
    /// Disables decoration for the current control.
    /// </summary>
    public void DoNotDecorate() => ForceNoDecorators = true;

    /// <summary>
    /// Retains the decorator queue for the next control.
    /// </summary>
    public void KeepForNextTime() => keepForNextStack++;

    /// <summary>
    /// Processes the decorators and applies them to the provided control instance.
    /// </summary>
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

    /// <summary>
    /// Adds a tooltip decorator with the specified message to the queue and returns the updated DecoratorQueue.
    /// </summary>
    public DecoratorQueue Tooltip(in string message)
    {
        Add(new Tooltip(message));
        return this;
    }
}
