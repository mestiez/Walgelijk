using System;
using System.Buffers;
using System.Numerics;

namespace Walgelijk;

public struct AudioAnalysis
{
    public static void BlurSignal(Span<float> values, float intensity)
    {
        var even = new Vector3(1 / 3f);
        var center = new Vector3(0, 1, 0);
        Convolve(values, Utilities.Lerp(center, even, intensity));
    }

    public static void Convolve(Span<float> values, Vector3 kernel)
    {
        var s = ArrayPool<float>.Shared.Rent(values.Length);
        var scrap = s.AsSpan(0, values.Length);
        values.CopyTo(scrap);
        for (int i = 1; i < values.Length - 1; i++)
            values[i] = (scrap[i - 1] * kernel.X + scrap[i + 1] * kernel.Y + scrap[i] * kernel.Z);
        ArrayPool<float>.Shared.Return(s);
    }

    public static void Convolve(Span<float> values, ReadOnlySpan<float> kernel)
    {
        if (kernel.Length % 2 == 0)
            throw new ArgumentException("Kernel must have an odd size");

        int kernelRadius = kernel.Length / 2;
        var s = ArrayPool<float>.Shared.Rent(values.Length);
        var scrap = s.AsSpan(0, values.Length);
        values.CopyTo(scrap);

        for (int i = kernelRadius; i < values.Length - kernelRadius; i++)
        {
            float sum = 0;
            for (int j = -kernelRadius; j <= kernelRadius; j++)
                sum += scrap[i + j] * kernel[kernelRadius + j];

            values[i] = sum;
        }

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

    public static float Hann(float v, int n, int N)
    {
        return v * (0.5f - 0.5f * MathF.Cos(2 * MathF.PI * n / (N - 1)));
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
            var v = (float)((m.Real * m.Real + m.Imaginary * m.Imaginary));
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
}