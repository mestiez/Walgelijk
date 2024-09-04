using System.Numerics;
using Walgelijk.PortAudio.Effects;

namespace Walgelijk.PortAudio.Voices;

internal interface IVoice 
{
    public double Time { get; set; }
    public bool IsFinished { get; }

    public List<IEffect> Effects { get; }

    public float Volume { get; set; }
    public float Pitch { get; set; }
    public Vector3 Position { get; set; }
    public SoundState State { get; set; }
    public AudioTrack? Track { get; }

    public void Play();
    public void Pause();
    public void Stop();

    public void GetSamples(Span<float> frame);
}
