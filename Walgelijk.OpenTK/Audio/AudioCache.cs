using OpenTK.Audio.OpenAL;
using System.Collections.Generic;

namespace Walgelijk.OpenTK
{
    internal class AudioCache : Cache<AudioData, int>
    {
        protected override int CreateNew(AudioData raw)
        {
            var buffer = AL.GenBuffer();

            //channel count can only be 1 or 2, as guaranteed by the file reader
            //bits per sample can only be 16 s guaranteed by the same lad
            ALFormat format = raw.ChannelCount == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;

            AL.BufferData(buffer, format, raw.Data, raw.SampleRate);
            if (!raw.KeepInMemory)
                raw.ForceClearData();

            return buffer;
        }

        protected override void DisposeOf(int loaded)
        {
            AL.DeleteBuffer(loaded);
        }
    }
}
