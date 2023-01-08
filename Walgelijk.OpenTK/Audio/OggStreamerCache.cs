namespace Walgelijk.OpenTK;

public class OggStreamerCache : Cache<(SourceHandle, Sound), OggStreamer>
{
    protected override OggStreamer CreateNew((SourceHandle, Sound) raw) => new(raw.Item1, raw.Item2.Data as StreamAudioData);

    protected override void DisposeOf(OggStreamer loaded)
    {
        loaded.Dispose();
    }
}
