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

public class DelayEffect : IEffect
{
    private Queue<float> delayBuffer = [];

    public void Process(Span<float> frame, ulong sampleIndex, TimeSpan time)
    {
        foreach (var sample in frame)
            delayBuffer.Enqueue(sample);

        int index = 0;
        while (delayBuffer.Count > 12000)
        {
            if (delayBuffer.TryDequeue(out var sample))
            {
                frame[index++] += sample * 0.5f;
                if (index >= frame.Length)
                    return;
            }
        }
    }
}