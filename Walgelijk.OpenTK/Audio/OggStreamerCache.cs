using System;

namespace Walgelijk.OpenTK;

public class OggStreamerCache : Cache<OggStreamerHandle, OggStreamer>
{
    protected override OggStreamer CreateNew(OggStreamerHandle raw)
    {
        return new OggStreamer(
            raw.SourceHandle, 
            raw.Sound, 
            raw.Sound.Data as StreamAudioData ?? throw new Exception("OggStreamer created with non streaming audio data source"));
    }

    protected override void DisposeOf(OggStreamer loaded)
    {
        loaded.Dispose();
    }
}
