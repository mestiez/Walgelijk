using System;

namespace Walgelijk;

/// <summary>
/// Audio generation and manipulation functions. These only work on 16 bit audio.
/// </summary>
public static class AudioGen
{
    public delegate float AudioFunction(TimeSpan duration, int sampleRate, float time);

    public static AudioFunction Sine(float amplitude, float freq)
        => (d, s, t) => amplitude * float.Sin(freq * float.Pi * t);

    public static AudioFunction Square(float amplitude, float freq)
        => (d, s, t) => amplitude * (float.Sin(freq * float.Pi * t) > 0 ? 1 : -1);

    public static AudioFunction Triangle(float amplitude, float freq)
        => (d, s, t) => amplitude * (2f / float.Pi) * float.Asin(float.Sin(freq * float.Pi * t));

    public static AudioFunction Sawtooth(float amplitude, float freq)
        => (d, s, t) => amplitude * (2 * (freq * t - float.Floor(freq * t + 0.5f)));

    public static AudioFunction WhiteNoise(float amplitude)
        => (d, s, t) => amplitude * Utilities.RandomFloat(-1, 1);

    public static AudioFunction BrownNoise(float amplitude)
    {
        var y = 0f;
        return (d, s, t) =>
        {
            y = 0.997f * y + 0.002f * Utilities.RandomFloat(-1, 1);
            return amplitude * y;
        };
    }

    public static FixedAudioData SignalGenerator(AudioFunction func, int sampleRate, TimeSpan duration)
    {
        var c = (int)(sampleRate * duration.TotalSeconds);
        var data = new float[c];
        for (int i = 0; i < c; i++)
        {
            var time = i / (float)sampleRate;
            data[i] = func(duration, sampleRate, time);
        }
        return new FixedAudioData(data, sampleRate, 2, data.Length, false);
    }
}