using OpenTK.Audio.OpenAL;
using System;

namespace Walgelijk.OpenTK;

internal class FixedAudioCache : Cache<FixedAudioData, BufferHandle>
{
    protected override BufferHandle CreateNew(FixedAudioData raw)
    {
        var buffer = AL.GenBuffer();

        ALUtils.CheckError("Generate new fixed buffer");

        // channel count can only be 1 or 2, as guaranteed by the file reader
        ALFormat format = raw.ChannelCount == 1 ? ALFormat.MonoFloat32Ext : ALFormat.StereoFloat32Ext;

        unsafe
        {
            var data = raw.GetData() ?? ReadOnlyMemory<float>.Empty;
            //using var coll = data.Pin();
            AL.BufferData(buffer, format, data.Span, raw.SampleRate);
           // coll.Dispose();
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
