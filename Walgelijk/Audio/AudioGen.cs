using System;

namespace Walgelijk;

/// <summary>
/// Audio generation and manipulation functions. These only work on 16 bit audio.
/// </summary>
public static class AudioGen
{
    public delegate double AudioFunction(TimeSpan duration, int sampleRate, double time);

    public static AudioFunction Sine(float amplitude, float freq)
        => (d, s, t) => amplitude * Math.Sin(freq * Math.PI * t);

    public static AudioFunction Square(float amplitude, float freq)
        => (d, s, t) => amplitude * (Math.Sin(freq * Math.PI * t) > 0 ? 1 : -1);

    public static AudioFunction Triangle(float amplitude, float freq)
        => (d, s, t) => amplitude * (2.0 / Math.PI) * Math.Asin(Math.Sin(freq * Math.PI * t));

    public static AudioFunction Sawtooth(float amplitude, float freq)
        => (d, s, t) => amplitude * (2 * (freq * t - Math.Floor(freq * t + 0.5)));

    public static AudioFunction WhiteNoise(float amplitude)
        => (d, s, t) => amplitude * Utilities.RandomFloat(-1, 1);

    public static AudioFunction BrownNoise(float amplitude)
    {
        double y = 0;
        return (d, s, t) =>
        {
            y = 0.997 * y + 0.002 * Utilities.RandomFloat(-1, 1);
            return amplitude * y;
        };
    }

    public static FixedAudioData FadeOut(this FixedAudioData data)
    {
        for (int i = 0; i < data.Data.Length; i += 2)
        {
            short sample = (short)(data.Data[i] | (data.Data[i + 1] << 8));
            var f = Utilities.MapRange(0, data.Data.Length, 1, 0, i);
            f = Easings.Quad.Out(f);
            sample = (short)(sample * f);
            data.Data[i] = (byte)(sample & 0xFF);
            data.Data[i + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return data;
    }

    public static FixedAudioData SignalGenerator(AudioFunction func, int sampleRate, TimeSpan duration)
    {
        var c = (int)(sampleRate * duration.TotalSeconds);
        var data = new byte[c * 2];
        for (int i = 0; i < c; i++)
        {
            double time = i / (double)sampleRate;
            int val = (int)Utilities.MapRange(-1, 1, -32768, 32767, func(duration, sampleRate, time));
            data[i * 2] = (byte)(val & 0xFF);
            data[i * 2 + 1] = (byte)(val >> 8);
        }
        return new FixedAudioData(data, sampleRate, 2, data.Length, false);
    }
}