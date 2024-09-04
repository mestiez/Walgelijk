using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.PortAudio.Effects;

public interface IEffect
{
    public void Process(Span<float> frame, ulong sampleIndex, TimeSpan time);
}

public class DistortionEffect : IEffect
{
    public float Distortion = 4;

    public void Process(Span<float> frame, ulong sampleIndex, TimeSpan time)
    {
        for (int i = 0; i < frame.Length; i++)
        {
            frame[i] = float.Clamp(frame[i] * Distortion, -1, 1);
        }
    }
}