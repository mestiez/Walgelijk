using OpenTK.Audio.OpenAL;
using System;

namespace Walgelijk.OpenTK;

public class SourceCache : Cache<Sound, SourceHandle>
{
    protected override SourceHandle CreateNew(Sound raw) => CreateSourceFor(raw);

    protected override void DisposeOf(SourceHandle loaded)
    {
        var buffer = AL.GetSource(loaded, ALGetSourcei.Buffer);
        if (AudioObjects.SourceByBuffer.TryGetValue(buffer, out var sourceList))
            sourceList.Remove(loaded);

        AL.SourceStop(loaded);
        AL.Source(loaded, ALSourcei.Buffer, 0);
        AL.DeleteSource(loaded);
    }

    public Sound GetSoundFor(SourceHandle handle)
    {
        foreach (var item in Loaded)
            if (item.Value == handle)
                return item.Key;
        throw new global::System.Exception("Attempt to get a sound for a handle that does not exist");
    }

    internal static SourceHandle CreateSourceFor(Sound sound)
    {
        ALUtils.CheckError();

        switch (sound.Data)
        {
            case FixedAudioData fixedData:
                {
                    var buffer = AudioObjects.FixedBuffers.Load(fixedData);
                    if (!AL.IsBuffer(buffer))
                        throw new Exception("Failed to create fixed audio buffer");
                    var source = AL.GenSource();
                    //ALUtils.CheckError();
                    if (!AL.IsSource(source))
                        throw new Exception("Failed to create fixed audio source");
                    AL.Source(source, ALSourcei.Buffer, buffer);
                    if (AudioObjects.SourceByBuffer.TryGetValue(buffer, out var sourceList))
                        sourceList.Add(source);
                    else
                        Logger.Error("Failed to register source to buffer: the buffer has no entry in the SourceByBuffer dictionary");

                    return source;
                }
            case StreamAudioData streamData:

                var s = AL.GenSource();

                if (!AL.IsSource(s))
                    throw new Exception("Failed to create streaming audio source");

                //var effect = ALC.EFX.GenEffect();
                //var slot = ALC.EFX.GenAuxiliaryEffectSlot();
                //ALC.EFX.Effect(effect, EffectInteger.EffectType, (int)EffectType.Reverb);
                //ALC.EFX.AuxiliaryEffectSlot(slot, EffectSlotInteger.Effect, effect);
                //ALC.EFX.Source(s, EFXSourceInteger3.AuxiliarySendFilter, new int[] { slot, 0, 0});

                AudioObjects.AudioStreamers.Load(new AudioStreamerHandle(s, sound));
                return s;
        }

        throw new global::System.Exception("AudioData of type " + sound.Data.GetType().Name + $" is not supported. Only {nameof(FixedAudioData)} and {nameof(StreamAudioData)} are understood.");
    }
}
