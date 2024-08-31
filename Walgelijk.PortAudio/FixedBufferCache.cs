namespace Walgelijk.PortAudio;

internal class FixedBufferCache : ConcurrentCache<FixedAudioData, float[]>
{
    public static FixedBufferCache Shared { get; } = new();
 
    protected override float[] CreateNew(FixedAudioData raw)
    {
        var data = new float[raw.SampleCount];

        // data is 16 bits per sample, interleaved
        // TODO resampling

        var p = 0;

        for (int i = 0; i < raw.Data.Length; i += 2)
        {
            var val = BitConverter.ToInt16(raw.Data.AsSpan(i, 2));
            data[p++] = (val / (float)short.MaxValue);
        }

        return data;
    }

    protected override void DisposeOf(float[] loaded)
    {

    }
}
