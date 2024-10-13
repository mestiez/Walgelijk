using System.Collections.Immutable;
using Walgelijk.PortAudio.Voices;

namespace Walgelijk.PortAudio;

internal class StreamBuffer : IDisposable, IStreamBuffer
{
    private readonly IAudioStream stream;
    private readonly int bufferSize;
    private readonly bool needsResampling;

    private double requestSetTime;
    private int currentSampleIndex;
    private StreamChunkFixedVoice? currentVoice = null;

    private Queue<StreamChunkFixedVoice> buffer = [];
    private SemaphoreSlim readingFromSource = new(1);
    private int workerCount;

    private int SourceSampleRate => stream.SampleRate;

    public StreamBuffer(IAudioStream stream, int bufferSize)
    {
        this.stream = stream;
        this.bufferSize = bufferSize;

        needsResampling = stream.SampleRate != PortAudioRenderer.SampleRate;
        //voices = new StreamChunkFixedVoice[4];
    }

    public double Time
    {
        get => requestSetTime != -1 ? requestSetTime : stream.TimePosition.TotalSeconds;
        set
        {
            requestSetTime = value;
        }
    }

    public ulong SampleIndex { get; private set; }

    public void GetSamples(Span<float> frame, float amplitude, float pitch)
    {
        if (requestSetTime != -1)
        {
            stream.TimePosition = TimeSpan.FromSeconds(requestSetTime);
            requestSetTime = -1;
        }

        while (buffer.Count < 4)
            buffer.Enqueue(GetNextSource()); // TODO put this on another thread

        int indexInFrame = 0;

        while (true)
        {
            if (currentVoice == null || currentVoice.IsFinished)
                if (!buffer.TryDequeue(out currentVoice))
                    break;

            currentVoice.Pitch = pitch;
            currentVoice.Volume = amplitude;
            currentVoice.GetSamples(frame[indexInFrame..]);
            indexInFrame += currentVoice.WrittenSamples;

            if (indexInFrame >= frame.Length - 1)
                break;
        }

        // TODO
        // there is some crackling here at low pitches
        // i SPECULATE this is because of interpolation between two chunks is not implemented
        // also resampling the streaming thing MAY cause some crackling too, for reasons i dont understand, but are probably the same as the speed/pitch thing (speed adjustment is identical to resampling)
    }

    private StreamChunkFixedVoice GetNextSource()
    {
        Interlocked.Increment(ref workerCount);
        readingFromSource.Wait();
        try
        {
            int offset = currentSampleIndex;

            var data = new float[bufferSize];
            var read = stream.ReadSamples(data);
            if (Random.Shared.NextSingle() > 0.8f)
                Thread.Sleep(Random.Shared.Next(0, 344));

            if (needsResampling)
            {
                float[] resampled = new float[Resampler.GetOutputLength(data.Length, stream.SampleRate, PortAudioRenderer.SampleRate, stream.ChannelCount)];
                if (stream.ChannelCount == 2)
                    Resampler.ResampleInterleavedStereo(data, resampled, stream.SampleRate, PortAudioRenderer.SampleRate);
                else
                    Resampler.ResampleMono(data, resampled, stream.SampleRate, PortAudioRenderer.SampleRate);
                data = resampled;
            }

            return new StreamChunkFixedVoice([.. data], stream.ChannelCount)
            {
                State = SoundState.Playing,
            };
        }
        finally
        {
            readingFromSource.Release(); 
            Interlocked.Decrement(ref workerCount);
        }
    }

    public void Clear()
    {
        requestSetTime = -1;
        stream.TimePosition = TimeSpan.Zero;
        SampleIndex = 0;
        buffer.Clear();
        currentVoice = null;
    }

    public void Dispose()
    {
        stream.Dispose();
    }

    internal class StreamChunkFixedVoice : FixedVoice
    {
        public override bool IsFinished => isFinished || SampleIndex >= (Data.Length / ChannelCount);
        private bool isFinished;

        public StreamChunkFixedVoice(ImmutableArray<float> data, int channelCount) : base(data)
        {
            ChannelCount = channelCount;
            Track = null;
        }

        public override float Volume { get; set; }
        public override float Pitch { get; set; }
        public override SoundState State { get; set; }
        public override bool Looping { get => false; set { } }
        public override int ChannelCount { get; }
        public override AudioTrack? Track { get; }

        public override void Pause()
        {
            State = SoundState.Paused;
        }

        public override void Play()
        {
            State = SoundState.Playing;
        }

        public override void Stop()
        {
            State = SoundState.Stopped;
            isFinished = true;
        }
    }

}
