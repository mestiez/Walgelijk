using OpenTK.Audio.OpenAL;

namespace Walgelijk.OpenTK;

internal class FixedAudioCache : Cache<FixedAudioData, BufferHandle>
{
    protected override BufferHandle CreateNew(FixedAudioData raw)
    {
        var buffer = AL.GenBuffer();

        ALUtils.CheckError("Generate new fixed buffer");

        // channel count can only be 1 or 2, as guaranteed by the file reader
        // bits per sample can only be 16 s guaranteed by the same lad
        ALFormat format = raw.ChannelCount == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;

        unsafe
        {
            var data = raw.GetData() ?? global::System.ReadOnlyMemory<byte>.Empty;
            using var coll = data.Pin();
            AL.BufferData(buffer, format, coll.Pointer, data.Length, raw.SampleRate);
            coll.Dispose();
            ALUtils.CheckError("Upload fixed buffer data");
        }

        if (raw.DisposeLocalCopyAfterUpload)
            raw.DisposeLocalCopy();

        AudioObjects.SourceByBuffer.Ensure(buffer);

        return buffer;
    }

    protected override void DisposeOf(BufferHandle loaded)
    {
        if (AudioObjects.SourceByBuffer.TryGetValue(loaded, out var sources))
            foreach (var s in sources)
            {
                AL.SourceStop(s);
                AL.Source(s, ALSourcei.Buffer, 0);
            }

        if (!AudioObjects.SourceByBuffer.TryRemove(loaded, out sources))
        {
            Logger.Error("Failed to clear SourceByBuffer dictionary");
            sources.Clear();
        }

        AL.DeleteBuffer(loaded);
    }
}
