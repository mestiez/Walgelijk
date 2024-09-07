namespace Walgelijk.PortAudio;

// TODO
// remove queue, favour preallocated arrays
// remove new arrays in FillBuffer, use preexisting arrays
// resampling should be given the previous frame to prevent clicking and popping
// resampler should not be a simple linear interpolation

internal class StreamBuffer : IDisposable
{
    private readonly IAudioStream Stream;
    private readonly int BufferSize;

    private readonly bool needsResampling;
    private readonly int resampledBufferSize;
    private readonly float[] readBuffer;

    private SemaphoreSlim readingFromSource = new(1);
    private int bufferPosition = 0;
    private double requestSetTime = -1;

    private readonly PriorityQueue<float[], double> decoded = new();
    private int workerCount = 0;

    private float[]? lastBuffer = null;
    private int lastBufferPos = 0;

    //private int SourceSampleRate => (int)(Stream.SampleRate * 0.25f);
    private int SourceSampleRate => Stream.SampleRate;

    public StreamBuffer(IAudioStream stream, int bufferSize)
    {
        Stream = stream;
        BufferSize = bufferSize;

        needsResampling = stream.SampleRate != PortAudioRenderer.SampleRate;
        resampledBufferSize = Resampler.GetInputLength(BufferSize, SourceSampleRate, PortAudioRenderer.SampleRate, stream.ChannelCount);
        readBuffer = new float[resampledBufferSize];
    }

    public double Time
    {
        get => requestSetTime != -1 ? requestSetTime : Stream.TimePosition.TotalSeconds;
        set
        {
            requestSetTime = value;
        }
    }

    public ulong SampleIndex { get; private set; }

    public void GetSamples(Span<float> frame, float multiplier)
    {
        // TODO deze structuur werkt niet als de resampled buffer kleiner is dan de frame omdat
        // je dan meerdere resampled buffers moet opvragen per GetSamples invocation

        if (lastBuffer == null)
        {
            lock (decoded)
            {
                if (decoded.TryDequeue(out lastBuffer, out _))
                    lastBufferPos = 0;
            }
            
            ThreadPool.QueueUserWorkItem(FillBuffer);
        }

        if (lastBuffer != null)
        {
            int position = 0;
            for (int i = lastBufferPos; i < lastBuffer.Length; i++)
            {
                frame[position++] += lastBuffer[i] * multiplier;
                lastBufferPos++;
                SampleIndex++;

                if (position >= frame.Length)
                {
                    if (i == lastBuffer.Length - 1)
                        returnBuffer();
                    return;
                }
            }

            returnBuffer();

            void returnBuffer()
            {
                lastBuffer = null;
            }
        }
    }

    private void FillBuffer(object? state)
    {
        Interlocked.Increment(ref workerCount);
        readingFromSource.Wait();
        try
        {
            if (requestSetTime != -1)
            {
                Stream.TimePosition = TimeSpan.FromSeconds(requestSetTime);
                requestSetTime = -1;
            }

            lock (decoded)
                if (needsResampling)
                {
                    Stream.ReadSamples(readBuffer);

                    var requestedLength = Resampler.GetOutputLength(resampledBufferSize, SourceSampleRate, PortAudioRenderer.SampleRate, Stream.ChannelCount);
                    var resampled = new float[requestedLength];
                    int read = 0;

                    if (Stream.ChannelCount == 2)
                        read = Resampler.ResampleInterleavedStereo(readBuffer, resampled, SourceSampleRate, PortAudioRenderer.SampleRate);
                    else
                        read = Resampler.ResampleMono(readBuffer, resampled, SourceSampleRate, PortAudioRenderer.SampleRate);

                    decoded.Enqueue(resampled, Stream.TimePosition.TotalSeconds);
                }
                else
                {
                    var buffer = new float[BufferSize];
                    var read = Stream.ReadSamples(buffer);
                    decoded.Enqueue(buffer, Stream.TimePosition.TotalSeconds);
                }
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
        Stream.TimePosition = TimeSpan.Zero;
        bufferPosition = 0;
        decoded.Clear();
        lastBuffer = null;
        SampleIndex = 0;
        //Array.Clear(readBuffer);
        //Array.Clear(writeBuffer);
    }

    public void Dispose()
    {
        Stream.Dispose();
    }
}
