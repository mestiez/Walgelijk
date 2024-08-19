using OpenTK.Audio.OpenAL;
using System.Collections.Concurrent;

namespace MotionTK;

public class AudioBuffer : IDisposable
{
    private readonly static ConcurrentBag<AudioBuffer> Free = new();
    private readonly static ConcurrentDictionary<int, AudioBuffer> byHandle = new();

    public readonly int Id;
    public readonly int Handle;

    public short[] Data = Array.Empty<short>();
    public ALFormat Format = ALFormat.Stereo8;
    public int SampleRate;

    private AudioBuffer(int size, ALFormat format, int sampleRate)
    {
        Id = byHandle.Count;
        Handle = AL.GenBuffer();


        Init(size, format, sampleRate);
        UploadData();

        byHandle.TryAdd(Handle, this);
    }

    public void Init(int size, ALFormat format, int sampleRate)
    {
        Format = format;
        SampleRate = sampleRate;
        if (!Resize(size)) Clear();
    }

    public void Clear()
    {
        if (Data != null)
            for (int i = 0; i < Data.Length; i++)
                Data[0] = 0;
    }

    public bool Resize(int size)
    {
        if (Data != null && Data.Length == size) return false;

        Array.Resize(ref Data, size);
        return true;
    }

    public void UploadData()
    {
        AL.BufferData(Handle, Format, Data, SampleRate);
    }

    public void MakeAvailable()
    {
        Free.Add(this);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        AL.DeleteBuffer(Handle);
        byHandle.TryRemove(Handle, out _);
    }

    ~AudioBuffer() => Dispose();

    public static AudioBuffer? ByHandle(int handle)
    {
        if (byHandle.TryGetValue(handle, out var b))
            return b;
        return null;
    }

    public static AudioBuffer Get(int size, ALFormat format, int sampleRate)
    {
        if (!Free.TryTake(out AudioBuffer? buffer))
            buffer = new AudioBuffer(size, format, sampleRate);
        buffer.Init(size, format, sampleRate);
        return buffer;
    }
}
