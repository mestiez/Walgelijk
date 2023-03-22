namespace Walgelijk.OpenTK;

public class OggStreamerCache : Cache<(SourceHandle, Sound), OggStreamer>
{
    protected override OggStreamer CreateNew((SourceHandle, Sound) raw)
        => new OggStreamer(raw.Item1, raw.Item2, raw.Item2.Data as StreamAudioData ?? throw new global::System.Exception("OggStreamer created with non streaming audio data sourcecool"));

    protected override void DisposeOf(OggStreamer loaded)
    {
        loaded.Dispose();
    }
}
