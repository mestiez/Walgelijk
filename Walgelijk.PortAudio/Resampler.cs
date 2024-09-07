namespace Walgelijk.PortAudio;

internal static class Resampler
{
    public static int GetOutputLength(int inputLength, int sampleRateIn, int sampleRateOut, int channelCount)
    {
        return ((int)(inputLength / channelCount * (double)sampleRateOut / sampleRateIn)) * channelCount;
    }

    public static int GetInputLength(int outputLength, int sampleRateIn, int sampleRateOut, int channelCount)
    {
        return (int)double.Ceiling((outputLength / (double)channelCount) * sampleRateIn / sampleRateOut) * channelCount;
    }

    public static int ResampleMono(ReadOnlySpan<float> input, Span<float> output, int sampleRateIn, int sampleRateOut)
    {
        if (sampleRateIn == sampleRateOut)
        {
            input.CopyTo(output);
            return 0;
        }

        var outputLength = GetOutputLength(input.Length, sampleRateIn, sampleRateOut, 1);
        if (output.Length < outputLength)
            throw new Exception("Output array should be of length " + outputLength);

        var ratio = (double)sampleRateIn / sampleRateOut;
        var position = 0d;

        for (int i = 0; i < outputLength; i++)
        {
            var index = (int)position;
            var frac = (float)(position - index);

            if (index + 1 < input.Length)
                output[i] = input[index] * (1 - frac) + input[index + 1] * frac;
            else
                output[i] = input[index];

            position += ratio;
        }

        return outputLength;
    }

    public static int ResampleInterleavedStereo(ReadOnlySpan<float> input, Span<float> output, int sampleRateIn, int sampleRateOut)
    {
        if (sampleRateIn == sampleRateOut)
        {
            input.CopyTo(output);
            return 0;
        }

        var outputLength = GetOutputLength(input.Length, sampleRateIn, sampleRateOut, 2);
        if (output.Length < outputLength)
            throw new Exception("Output array should be of length " + outputLength);

        var ratio = (double)sampleRateIn / sampleRateOut;
        var position = 0d;

        for (int i = 0; i < outputLength; i += 2)
        {
            var index = (int)position * 2;
            var frac = (float)(position - index / 2);

            if (index + 2 < input.Length) 
            {
                output[i] = input[index] * (1 - frac) + input[index + 2] * frac;
                output[i + 1] = input[index + 1] * (1 - frac) + input[index + 3] * frac;
            }
            else
            {
                output[i] = input[index];
                output[i + 1] = input[index + 1];
            }

            position += ratio;
        }

        return outputLength;
    }
}