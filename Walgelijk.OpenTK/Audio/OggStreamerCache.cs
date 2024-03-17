using System;

namespace Walgelijk.OpenTK;

public class OggStreamerCache : Cache<OggStreamerHandle, AudioStreamer>
{
    protected override AudioStreamer CreateNew(OggStreamerHandle raw)
    {
        return new AudioStreamer(
            raw.SourceHandle, 
            raw.Sound, 
            raw.Sound.Data as StreamAudioData ?? throw new Exception("OggStreamer created with non streaming audio data source"));
    }

    protected override void DisposeOf(AudioStreamer loaded)
    {
        loaded.Dispose();
    }
}
