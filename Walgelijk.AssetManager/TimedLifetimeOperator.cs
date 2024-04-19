
namespace Walgelijk.AssetManager;

public class TimedLifetimeOperator : ILifetimeOperator
{
    public Hook Triggered { get; } = new();

    public TimedLifetimeOperator(TimeSpan t)
    {
        RoutineScheduler.Start(Delayed(t));
    } 
    
    public TimedLifetimeOperator(float seconds)
    {
        RoutineScheduler.Start(Delayed(TimeSpan.FromSeconds(seconds)));
    }

    private IEnumerator<IRoutineCommand> Delayed(TimeSpan t)
    {
        yield return new RoutineDelay((float)t.TotalSeconds);
        Triggered.Dispatch();
        Triggered.ClearListeners();
    }
}