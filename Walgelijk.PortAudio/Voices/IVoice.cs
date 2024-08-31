using System.Numerics;

namespace Walgelijk.PortAudio.Voices;

internal interface IVoice 
{
    public double Time { get; set; }
    public Vector3 Position { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsFinished { get; }

    public void Play();
    public void Pause();
    public void Stop();

    public void GetSamples(Span<float> frame);
}
