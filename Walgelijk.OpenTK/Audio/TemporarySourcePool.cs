namespace Walgelijk.OpenTK;

internal class TemporarySourcePool : Pool<TemporarySource?, TemporarySourceArgs>
{
    public TemporarySourcePool(int maxCapacity) : base(maxCapacity)
    {
    }

    protected override TemporarySource? CreateFresh() => new();

    protected override TemporarySource? GetOverCapacityFallback() => null;

    protected override void ResetObjectForNextUse(TemporarySource? obj, TemporarySourceArgs initialiser)
    {
        obj.Sound = initialiser.Sound;
        obj.Source = initialiser.Source;
        obj.Duration = initialiser.Duration;
        obj.Volume = initialiser.Volume;
        obj.CurrentLifetime = 0;
    }
}
