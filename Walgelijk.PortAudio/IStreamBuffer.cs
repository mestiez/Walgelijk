
namespace Walgelijk.PortAudio;

internal interface IStreamBuffer
{
    ulong SampleIndex { get; }
    double Time { get; set; }

    void Clear();
    void Dispose();
    void GetSamples(Span<float> frame, float multiplier, float pitch);
}