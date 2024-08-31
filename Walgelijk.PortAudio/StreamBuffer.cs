namespace Walgelijk.PortAudio;

internal class StreamBuffer(IAudioStream Stream, float[] Output) : IDisposable
{
    private readonly float[] writeBuffer = new float[Output.Length];

    public double Time
    { 
        get => Stream.TimePosition.TotalSeconds;
        set => Stream.TimePosition = TimeSpan.FromSeconds(Time);
    }

    public void Update()
    {
        int c = Stream.ReadSamples(writeBuffer);
    }

    public void Dispose()
    {
        Stream.Dispose();
    }
}
