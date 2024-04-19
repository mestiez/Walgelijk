using System;

namespace Walgelijk.OpenTK;

public class AudioStreamerCache : Cache<AudioStreamerHandle, AudioStreamer>
{
    protected override AudioStreamer CreateNew(AudioStreamerHandle raw)
    {
        return new AudioStreamer(
            raw.SourceHandle, 
            raw.Sound, 
            raw.Sound.Data as StreamAudioData ?? throw new Exception("AudioStreamer created with non streaming audio data source"));
    }

    protected override void DisposeOf(AudioStreamer loaded)
    {
        loaded.Dispose();
    }
}
