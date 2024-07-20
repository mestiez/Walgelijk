namespace Walgelijk.PLMPEG;

internal class MediaBuffer<T>
{
    private readonly T[] Buffer;

    private long writeCursor, readCursor;
    private readonly Mutex bufferLock = new();

    public long WriteCursorWrapped => writeCursor % Buffer.Length;
    public long ReadCursorWrapped => readCursor % Buffer.Length; 
    
    public long WriteCursor => writeCursor;
    public long ReadCursor => readCursor;

    public long Gap => writeCursor - readCursor;

    public MediaBuffer(T[] buffer)
    {
        Buffer = buffer;
    }

    public void Push(ReadOnlySpan<T> source)
    {
        bufferLock.WaitOne();
        try
        {
            for (int i = 0; i < source.Length; i++)
            {
                Buffer[WriteCursorWrapped] = source[i];
                writeCursor++;

                if (Gap >= Buffer.Length || Gap < 0)
                {
                    Console.Error.WriteLine("We are writing where we are reading. The buffer is too small or we are reading too fast. Gap: {0}", Gap);
                    writeCursor = 0;
                    readCursor = 0;
                }

            }
        }
        finally
        {
            bufferLock.ReleaseMutex();

        }
    }

    public void Take(Span<T> destination)
    {
        bufferLock.WaitOne();
        try
        {
            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = Buffer[ReadCursorWrapped];
                readCursor++;

                if (Gap >= Buffer.Length || Gap < 0)
                {
                    Console.Error.WriteLine("We are reading where we are writing. The buffer is too small or we are not writing fast enough. Gap: {0}", Gap);
                    readCursor = writeCursor - 1;
                }
            }
        }
        finally
        {
            bufferLock.ReleaseMutex();
        }
    }
}
