namespace Walgelijk.PortAudio;

internal class FixedBufferCache : ConcurrentCache<FixedAudioData, float[]>
{
    public static FixedBufferCache Shared { get; } = new();
 
    protected override float[] CreateNew(FixedAudioData raw)
    {
        // TODO resampling
        return [.. raw.Data];
    }

    protected override void DisposeOf(float[] loaded)
    {

    }
}
