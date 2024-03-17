using System;
using NVorbis;

namespace Walgelijk.OpenTK;

public class OggAudioStream : IAudioStream
{
    private readonly VorbisReader reader;

    public OggAudioStream(string path)
    {
        reader = new VorbisReader(path);
    }

    public long Position
    {
        get => reader.SamplePosition;
        set => reader.SamplePosition = value;
    }
    
    public TimeSpan TimePosition   
    {
        get => reader.TimePosition;
        set => reader.TimePosition = value;
    }
    
    public int ReadSamples(Span<float> b) => reader.ReadSamples(b);

    public void Dispose()
    {
        reader?.Dispose();
    }
}