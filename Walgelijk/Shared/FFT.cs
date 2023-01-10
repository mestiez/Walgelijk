using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Walgelijk;

public class AudioVisualiser
{
    public readonly Sound Sound;

    public readonly int BufferSize;
    public readonly int BinCount;
    public readonly int FftSize;
    public float MinFreq = 20;
    public float MaxFreq = 16000;
    public int InputBlur = 0;
    public int OutputBlur = 2;

    private readonly byte[] samples;
    private readonly float[] samplesCast;

    private readonly float[] fft;

    private readonly float[] sampleAccumulator;

    private readonly Bin[] bins;
    private readonly float[] visualiser;

    private int accumulationCursor = 0;

    private class Bin
    {
        public float Value = 0;
        public List<float> Section = new();
        //public Memory<float> Section;

        //public Bin(Memory<float> section)
        //{
        //    Section = section;
        //}
    }

    public AudioVisualiser(Sound sound, int fftSize = 4096, int bufferSize = 1024, int binCount = 128)
    {
        BufferSize = bufferSize;
        BinCount = binCount;
        Sound = sound;
        FftSize = fftSize;

        sampleAccumulator = new float[FftSize];
        samplesCast = new float[BufferSize];
        samples = new byte[BufferSize];
        fft = new float[FftSize];

        visualiser = new float[BinCount];
        bins = new Bin[BinCount];
        for (int i = 0; i < bins.Length; i++)
            bins[i] = new Bin();
    }

    public int FrequencyToFftIndex(float freq) => (int)((Sound.Data.SampleRate / (float)FftSize) / freq);
    public int FftIndexToBinIndex(int i, float gamma = 2) => (int)(MathF.Pow(i / (float)FftSize / 2, 1f / gamma) * BinCount);

    private void UpdateFft(AudioRenderer audio)
    {
        int readSampleCount = audio.GetCurrentSamples(Sound, samples);
        for (int i = 0; i < readSampleCount; i++)
            samplesCast[i] = Utilities.MapRange(0, byte.MaxValue - 1, -0.5f, 0.5f, samples[i]);

        if (accumulationCursor < FftSize) // if the amount of accumulated samples have not yet reached the end 
        {
            if (Sound.Data.ChannelCount == 2)
            {
                //stereo! channels are interleaved! take left
                for (int i = 0; i < samplesCast.Length; i += 2)
                    sampleAccumulator[accumulationCursor++] = samplesCast[i];
            }
            else
            {
                //mono! take everything as-is
                samplesCast.CopyTo(sampleAccumulator, accumulationCursor);
                accumulationCursor += readSampleCount;
            }
        }
        else
        {
            for (int i = 0; i < InputBlur; i++)
                AudioAnalysis.BlurSignal(sampleAccumulator);

            var actualFft = AudioAnalysis.Fft(sampleAccumulator, fft);
            accumulationCursor = 0;
        }
    }

    private void PopulateBins(float dt)
    {
        foreach (var item in bins)
        {
            item.Section.Clear();
        }

        for (int i = 0; i < FftSize / 2; i++)
        {
            var bin = bins[FftIndexToBinIndex(i)];
            bin.Section.Add(fft[i]);
        }

        foreach (var item in bins)
        {
            item.Value = Utilities.SmoothApproach(item.Value, item.Section.Any() ? (item.Section.Max()) : 0, 32, dt);
        }

        //for (int i = 0; i < BinCount; i++)
        //{
        //    float min = Utilities.MapRange(0, BinCount - 1, MinFreq, MaxFreq, i);
        //    float max = Utilities.MapRange(0, BinCount - 1, MinFreq, MaxFreq, i + 1);

        //    FftIndexToBinIndex
        //}
    }

    public void Update(AudioRenderer audio, float dt)
    {
        UpdateFft(audio);
        PopulateBins(dt);

        for (int i = 0; i < visualiser.Length; i++)
        {
            visualiser[i] = Utilities.SmoothApproach(visualiser[i], bins[i].Value, 64, dt);
        }

        for (int i = 0; i < OutputBlur; i++)
            AudioAnalysis.BlurSignal(visualiser);
    }

    public ReadOnlySpan<float> GetVisualiserData() => visualiser;
}

public struct AudioAnalysis
{
    public static void BlurSignal(Span<float> values)
    {
        var s = ArrayPool<float>.Shared.Rent(values.Length);
        var scrap = s.AsSpan(0, values.Length);
        values.CopyTo(scrap);
        for (int i = 1; i < values.Length - 1; i++)
            values[i] = (scrap[i - 1] + scrap[i + 1] + scrap[i]) / 3f;
        ArrayPool<float>.Shared.Return(s);
    }

    public static float Rectangular(float v, int n, int N)
    {
        return v;
    }

    public static float Blackman(float v, int n, int N)
    {
        return v * (0.42f - 0.5f * MathF.Cos(2 * MathF.PI * n / (N - 1)) + 0.08f * MathF.Cos(4 * MathF.PI * n / (N - 1)));
    }

    public static float Hamming(float v, int n, int N)
    {
        return v * (0.54f - 0.46f * MathF.Cos(2 * MathF.PI * n / (N - 1)));
    }

    public static Span<float> Fft(ReadOnlySpan<float> input, Span<float> result)
    {
        if (input.Length != result.Length)
            throw new Exception("input length and result length should be equal");
        bool isPowerOfTwo = (result.Length > 0) && ((result.Length & (result.Length - 1)) == 0);

        if (!isPowerOfTwo)
            throw new Exception("Length should be a power of two");

        int n = input.Length;

        var samples = ArrayPool<Complex>.Shared.Rent(n);
        var fft = ArrayPool<Complex>.Shared.Rent(n);

        for (int i = 0; i < n; i++)
            samples[i] = new Complex(Blackman(input[i], i, n), 0);

        Fft(samples, fft);

        for (int i = 0; i < n; i++)
        {
            var m = fft[i];
            var v = (float)(10d * Math.Log10(m.Real * m.Real + m.Imaginary * m.Imaginary));
            if (float.IsInfinity(v) || v < 0)
                v = 0;
            result[i] = v;
        }

        ArrayPool<Complex>.Shared.Return(samples, true);
        ArrayPool<Complex>.Shared.Return(fft, true);

        return result[0..(n / 2)];
    }

    public static void Fft(ReadOnlySpan<Complex> arr, Span<Complex> output)
    {
        var length = arr.Length;
        var hLength = length >> 1; //length is power of two dus length / 2 is hetzelfde als bitshift
        if (length == 1)
        {
            arr.CopyTo(output);
            return;
        }

        var eca = ArrayPool<Complex>.Shared.Rent(hLength);
        var oda = ArrayPool<Complex>.Shared.Rent(hLength);
        var ra = ArrayPool<Complex>.Shared.Rent(length);

        var evenCoefficients = eca.AsSpan(0, hLength);
        var oddCoefficients = oda.AsSpan(0, hLength);
        var roots = ra.AsSpan(0, length);

        for (var i = 0; i < length; i++)
        {
            var th = Math.Tau * (i / (double)length);
            roots[i] = new Complex(Math.Cos(th), Math.Sin(th));
        }
        for (var i = 0; i < hLength; i++)
        {
            evenCoefficients[i] = arr[i * 2];
            oddCoefficients[i] = arr[i * 2 + 1];
        }

        var eoca = ArrayPool<Complex>.Shared.Rent(hLength);
        var ooca = ArrayPool<Complex>.Shared.Rent(hLength);
        var evenOrderCoefficientTransform = eoca.AsSpan(0, hLength);
        var oddOrderCoefficientTransform = ooca.AsSpan(0, hLength);

        Fft(evenCoefficients, evenOrderCoefficientTransform);
        Fft(oddCoefficients, oddOrderCoefficientTransform);

        for (var i = 0; i < hLength; i++)
        {
            output[i] = evenOrderCoefficientTransform[i] + roots[i] * oddOrderCoefficientTransform[i];
            output[i + hLength] = evenOrderCoefficientTransform[i] - roots[i] * oddOrderCoefficientTransform[i];
        }

        ArrayPool<Complex>.Shared.Return(eca);
        ArrayPool<Complex>.Shared.Return(oda);
        ArrayPool<Complex>.Shared.Return(ra);
        ArrayPool<Complex>.Shared.Return(eoca);
        ArrayPool<Complex>.Shared.Return(ooca);
    }

    //public static void Fft(ReadOnlySpan<Complex> arr, Span<Complex> result)
    //{
    //    var length = arr.Length;
    //    var hLength = length >> 1;
    //    if (length == 1)
    //    {
    //        result[0] = arr[0];
    //        return;
    //    }
    //    var evenCoefficients = ArrayPool<Complex>.Shared.Rent(hLength);
    //    var oddCoefficients = ArrayPool<Complex>.Shared.Rent(hLength);
    //    var roots = ArrayPool<Complex>.Shared.Rent(length);
    //    var y = ArrayPool<Complex>.Shared.Rent(length);
    //    for (var i = 0; i < length; i++)
    //    {
    //        var alpha = 2 * Math.PI * i / length;
    //        roots[i] = new Complex(Math.Cos(alpha), Math.Sin(alpha));
    //    }
    //    for (var i = 0; i < hLength; i++)
    //    {
    //        evenCoefficients[i] = arr[i * 2];
    //        oddCoefficients[i] = arr[i * 2 + 1];
    //    }

    //    Fft(evenCoefficients, y);
    //    Fft(oddCoefficients, y.AsSpan(0, hLength));

    //    for (var i = 0; i < hLength; i++)
    //    {
    //        result[i] = y[i] + roots[i] * y[i + hLength];
    //        result[i + hLength] = y[i] - roots[i] * y[i + hLength];
    //    }

    //    ArrayPool<Complex>.Shared.Return(evenCoefficients, true);
    //    ArrayPool<Complex>.Shared.Return(oddCoefficients, true);
    //    ArrayPool<Complex>.Shared.Return(roots, true);
    //    ArrayPool<Complex>.Shared.Return(y, true);
    //}
}
#if false
public struct AudioAnalysis
{
    public static void FFT(ReadOnlySpan<byte> samples, Span<float> output)
    {
        var rental = ArrayPool<Complex>.Shared.Rent(output.Length);

        RawFFT(samples, rental);
        for (int i = 0; i < samples.Length; i++)
            output[i] = (float)Math.Sqrt(rental[i].Real * rental[i].Real + rental[i].Imaginary * rental[i].Imaginary);

        ArrayPool<Complex>.Shared.Return(rental);
    }

    public static void RawFFT(ReadOnlySpan<byte> rawSamples, Span<Complex> fft, bool window = true)
    {
        if (rawSamples.Length != fft.Length)
            throw new Exception($"{nameof(rawSamples)} is not the same length as {nameof(fft)}");

        int n = rawSamples.Length;

        if (n == 1)
        {
            fft[0] = new Complex(rawSamples[0], 0);
            return;
        }

        byte[] windowedSamples = ArrayPool<byte>.Shared.Rent(n);
        for (int i = 0; i < n; i++)
        {
            if (window)
                windowedSamples[i] = (byte)(rawSamples[i] * (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (n - 1))));
            else
                windowedSamples[i] = rawSamples[i];
        }

        byte[] even = ArrayPool<byte>.Shared.Rent(n / 2);
        byte[] odd = ArrayPool<byte>.Shared.Rent(n / 2);

        for (int i = 0; i < n / 2; i++)
        {
            even[i] = windowedSamples[i * 2];
            odd[i] = windowedSamples[i * 2 + 1];
        }

        Complex[] fftEven = ArrayPool<Complex>.Shared.Rent(n / 2);
        Complex[] fftOdd = ArrayPool<Complex>.Shared.Rent(n / 2);
        RawFFT(even.AsSpan(0, n / 2), fftEven.AsSpan(0, n / 2), false);
        RawFFT(odd.AsSpan(0, n / 2), fftOdd.AsSpan(0, n / 2), false);

        ArrayPool<byte>.Shared.Return(even, true);
        ArrayPool<byte>.Shared.Return(odd, true);
        ArrayPool<Complex>.Shared.Return(fftEven, true);
        ArrayPool<Complex>.Shared.Return(fftOdd, true);
        ArrayPool<byte>.Shared.Return(windowedSamples, true);

        for (int k = 0; k < n / 2; k++)
        {
            Complex t = new Complex(0, -2 * Math.PI * k / n) * fftOdd[k];
            fft[k] = fftEven[k] + t;
            fft[k + n / 2] = fftEven[k] - t;
        }
    }
}
#endif