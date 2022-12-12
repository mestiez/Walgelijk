namespace Walgelijk.ParticleSystem.Emitters;

public struct ContinuousEmitter : IParticleEmitter
{
    public bool Disabled { get; set; }
    public float RateHz
    {
        get => distributor.Rate;
        set => distributor.Rate = RateHz;
    }

    private readonly FixedIntervalDistributor distributor;

    public ContinuousEmitter(float rateHz) : this()
    {
        distributor = new(rateHz);
        Disabled = false;
    }

    public int Emit(in GameState gameState, ParticlesComponent component)
    {
        return distributor.CalculateCycleCount(gameState.Time.DeltaTime * component.SimulationSpeed);
    }
}

public struct BurstEmitter : IParticleEmitter
{
    public bool Disabled { get; set; }
    public float RateHz
    {
        get => distributor.Rate;
        set => distributor.Rate = RateHz;
    }

    public IntRange AmountPerBurst;

    private readonly FixedIntervalDistributor distributor;

    public BurstEmitter(float rateHz, int amountPerBurst) : this()
    {
        distributor = new(rateHz);
        AmountPerBurst = new(amountPerBurst);
        Disabled = false;
    }

    public BurstEmitter(float rateHz, int minPerBurst, int maxPerBurst)
    {
        distributor = new(rateHz);
        AmountPerBurst = new(minPerBurst, maxPerBurst);
        Disabled = false;
    }

    public int Emit(in GameState gameState, ParticlesComponent component)
    {
        return distributor.CalculateCycleCount(gameState.Time.DeltaTime * component.SimulationSpeed) * AmountPerBurst.GetRandom();
    }
}
