namespace Walgelijk.PortAudio;

internal class StreamBuffer(IAudioStream Stream, uint BufferSize) : IDisposable
{
    private readonly float[] writeBuffer = new float[BufferSize];
    private readonly float[] readBuffer = new float[BufferSize];

    private SemaphoreSlim readingFromSource = new(1);
    private int bufferPosition = 0;

    public double Time
    {
        get => Stream.TimePosition.TotalSeconds;
        set => Stream.TimePosition = TimeSpan.FromSeconds(value);
    }

    public void GetSamples(Span<float> frame)
    {
        for (int i = 0; i < frame.Length; i++)
        {
            frame[i] += readBuffer[(i + bufferPosition) % readBuffer.Length];
        }

        bufferPosition += frame.Length; 
        // this only works because the buffer size is divisible by the framesize (this is why they all have to be a power of two)
        // another constraint is that the frame size must never exceed the buffer size

        if (bufferPosition >= BufferSize) // last buffer was fully played so we need to fill the next one
        {
            bufferPosition = 0;
            writeBuffer.CopyTo(readBuffer, 0);

            ThreadPool.QueueUserWorkItem(FillBuffer); // TODO loop
        }
    }

    private void FillBuffer(object? state)
    {
        readingFromSource.Wait();
        try
        {
            Stream.ReadSamples(writeBuffer);
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
        finally
        {
            readingFromSource.Release();
        }
    }

    public void Clear()
    {
        Array.Clear(readBuffer);
        Array.Clear(writeBuffer);
    }

    public void Dispose()
    {
        Stream.Dispose();
    }
}
