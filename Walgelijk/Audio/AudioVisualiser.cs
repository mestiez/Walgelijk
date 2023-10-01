using System;

namespace Walgelijk;

public class AudioVisualiser
{
    public readonly Sound Sound;

    public readonly int BufferSize;
    public readonly int BinCount;
    public readonly int BarCount;
    public readonly int FftSize;
    public float MinFreq = 100;
    public float MaxFreq = 16000;
    public int InputBlurIterations = 0;
    public int OutputBlurIterations = 0;
    public float InputBlurIntensity = 0;
    public float OutputBlurIntensity = 0;
    public float Smoothing = 0.5f;
    public bool OverlapWindow = false;

    public float MinDb = -30;
    public float MaxDb = 100;

    private readonly float[] samples;
    private readonly float[] fft;
    private readonly float[] sampleAccumulator;

    public float SampleAccumulatorOverwriteFactor = 1;
    public float FalloffSpeed = 2;

    private readonly Bin[] bins;
    private readonly float[] smoothedBars;
    private readonly float[] bars;

    private int accumulationCursor = 0;

    private class Bin
    {
        public float Frequency;
        public Memory<float> Section;
        public int Start;

        public float Value = 0;
        public float LinearPeakSmoothedValue = 0;

        public Bin(Memory<float> section)
        {
            Section = section;
        }

        public float CalculateValue()
        {
            float v = 0;
            var span = Section.Span;
            //for (int i = 0; i < Section.Length; i++)
            //    v += span[i] / Section.Length;
            for (int i = 0; i < Section.Length; i++)
                v += span[i] * span[i];
            v = MathF.Sqrt(v / Section.Span.Length);
            return v;
        }
    }

    public AudioVisualiser(Sound sound, int fftSize = 64, int bufferSize = 1024, int barCount = 128)
    {
        BufferSize = bufferSize;
        BinCount = fftSize / 2;
        BarCount = barCount;
        Sound = sound;
        FftSize = fftSize;

        samples = new float[BufferSize];
        sampleAccumulator = new float[FftSize];
        fft = new float[FftSize];

        bars = new float[BarCount];
        smoothedBars = new float[BarCount];
        bins = new Bin[BinCount];
        for (int i = 0; i < bins.Length; i++)
            bins[i] = new Bin(Memory<float>.Empty) { Frequency = BinIndexToFrequency(i) };
    }

    public int FrequencyToBinIndex(float frequency)
    {
        float binWidth = Sound.Data.SampleRate / (float)FftSize;
        return Math.Clamp((int)Math.Floor(frequency / binWidth), 0, BinCount - 1);
    }

    public float BinIndexToFrequency(int binIndex)
    {
        float binWidth = Sound.Data.SampleRate / (float)FftSize;
        return binIndex * binWidth;
    }

    private void UpdateFft(AudioRenderer audio)
    {
        int readSampleCount = audio.GetCurrentSamples(Sound, samples);

        if (Sound.Data.ChannelCount == 2)
        {
            //stereo! channels are interleaved! take left
            for (int i = 0; i < readSampleCount; i += 2)
            {
                var index = accumulationCursor % sampleAccumulator.Length;
                sampleAccumulator[index] = Utilities.Lerp(sampleAccumulator[index], (samples[i] + samples[i + 1]) / 2f, SampleAccumulatorOverwriteFactor);
                accumulationCursor++;
            }
        }
        else
        {
            //mono! take everything as-is
            for (int i = 0; i < readSampleCount; i++)
            {
                var index = accumulationCursor % sampleAccumulator.Length;
                sampleAccumulator[index] = Utilities.Lerp(sampleAccumulator[index], samples[i], SampleAccumulatorOverwriteFactor);
                accumulationCursor++;
            }
        }

        if (accumulationCursor >= FftSize) // if the amount of accumulated samples have not yet reached the end 
        {
            for (int i = 0; i < InputBlurIterations; i++)
                AudioAnalysis.BlurSignal(sampleAccumulator, InputBlurIntensity);

            AudioAnalysis.Fft(sampleAccumulator, fft);

            if (OverlapWindow)
            {
                //shift sampleAccumulator to the left halfway to let the windows overlap
                sampleAccumulator.AsSpan(sampleAccumulator.Length - BufferSize / 2, BufferSize / 2).CopyTo(sampleAccumulator);
                accumulationCursor = BufferSize / 2;
            }
            else
                accumulationCursor = 0;
        }
    }

    private void PopulateBins()
    {
        foreach (var item in bins)
        {
            item.Section = Memory<float>.Empty;
            item.Start = -1;
        }

        for (int i = 0; i < BinCount; i++)
        {
            var bin = bins[i];
            if (bin.Section.Length == 0 || bin.Start == -1) // empty bin
            {
                bin.Section = fft.AsMemory(i, 1);
                bin.Start = i;
            }
            else
            {
                bin.Section = fft.AsMemory(bin.Start, i - bin.Start);
            }
        }

        foreach (var item in bins)
            item.Value = item.CalculateValue();
    }

    private float DecibelScale(float value)
    {
        float db = 20 * MathF.Log10(value);
        db = Utilities.Clamp(db, MinDb, MaxDb);
        float normalizedDb = Utilities.MapRange(MinDb, MaxDb, 0, 1, db);

        return Easings.Quad.In(normalizedDb);
    }

    public void Update(AudioRenderer audio, float dt)
    {
        UpdateFft(audio);
        PopulateBins();

        foreach (var b in bins)
        {
            var db = DecibelScale(b.Value);
            if (db > b.LinearPeakSmoothedValue)
                b.LinearPeakSmoothedValue = db;
            else
                b.LinearPeakSmoothedValue = MathF.Max(0, b.LinearPeakSmoothedValue - dt * FalloffSpeed);
        }

        float s = 35 / Smoothing;

        for (int i = 0; i < bars.Length; i++)
        {
            float f = (float)i / (bars.Length - 1);
            float freq = MinFreq * MathF.Pow(MaxFreq / MinFreq, f);

            var binIndex = FrequencyToBinIndex(freq);
            var currentBin = bins[binIndex];
            var nextBin = binIndex + 1 == bins.Length ? currentBin : bins[binIndex + 1];

            float value;

            if (currentBin == nextBin)
                value = currentBin.LinearPeakSmoothedValue;
            else
            {
                var a = currentBin.LinearPeakSmoothedValue;
                var b = nextBin.LinearPeakSmoothedValue;
                var t = Utilities.MapRange(currentBin.Frequency, nextBin.Frequency, 0, 1, freq);
                value = Utilities.Lerp(a, b, t);
            }
            bars[i] = value;
            smoothedBars[i] = Utilities.SmoothApproach(smoothedBars[i], bars[i], s, dt);
        }

        for (int i = 0; i < OutputBlurIterations; i++)
            AudioAnalysis.BlurSignal(smoothedBars, OutputBlurIntensity);
    }

    public ReadOnlySpan<float> GetVisualiserData() => smoothedBars;
}
