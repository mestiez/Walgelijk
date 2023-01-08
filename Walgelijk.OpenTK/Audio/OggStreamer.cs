using NVorbis;
using OpenTK.Audio.OpenAL;
using System;
using System.IO;

namespace Walgelijk.OpenTK;

public class OggStreamer : IDisposable
{
    public const int BufferSize = 1024;
    public const int MaxBufferCount = 32;

    public bool ShouldPlay => Sound.State is SoundState.Playing;
    public ALFormat Format => Raw.ChannelCount == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
    public readonly SourceHandle SourceHandle;
    public readonly Sound Sound;
    public readonly StreamAudioData Raw;
    public readonly BufferEntry[] Buffers = new BufferEntry[MaxBufferCount];

    public TimeSpan CurrentTime
    {
        get => reader.TimePosition;

        set
        {
            reader.TimePosition = value > TimeSpan.Zero ? (value < reader.TotalTime ? value : reader.TotalTime) : TimeSpan.Zero;
        }
    }

    private FileStream stream;
    private VorbisReader reader;
    private bool endReached;
    private readonly short[] readBuffer = new short[BufferSize]; //16 bits per sample
    private readonly float[] rawOggBuffer = new float[BufferSize];

    public float[] LastSamples = new float[BufferSize];

    public class BufferEntry
    {
        public readonly BufferHandle Handle;
        public bool Free;

        public BufferEntry(BufferHandle bufferHandle, bool free)
        {
            Handle = bufferHandle;
            Free = free;
        }
    }

    public OggStreamer(SourceHandle sourceHandle, Sound sound, StreamAudioData raw)
    {
        Raw = raw;
        SourceHandle = sourceHandle;
        Sound = sound;

        for (int i = 0; i < MaxBufferCount; i++)
            Buffers[i] = new BufferEntry(AL.GenBuffer(), true);

        Reset();
        Precache();
    }

    public void Reset()
    {
        endReached = false;
        stream?.Dispose();
        reader?.Dispose();

        stream = new FileStream(Raw.File.FullName, FileMode.Open, FileAccess.Read);
        reader = new VorbisReader(stream, false);

        Array.Clear(LastSamples);
    }

    public void Precache()
    {
        foreach (var item in Buffers)
            item.Free = true;

        AL.GetSource(SourceHandle, ALGetSourcei.BuffersProcessed, out int processed);
        for (int p = 0; p < processed; p++)
            AL.SourceUnqueueBuffer(SourceHandle);

        int queued = 0;
        while (queued++ < MaxBufferCount && TryRead(out var readAmount))
        {
            if (!TryGetFreeBuffer(out var buffer))
                continue;
            buffer.Free = false;
            CastBuffer(rawOggBuffer, readBuffer, readAmount);
            AL.BufferData<short>(buffer.Handle, Format, readBuffer.AsSpan(0, readAmount), Raw.SampleRate);
            AL.SourceQueueBuffer(SourceHandle, buffer.Handle);
        }
    }

    private bool TryGetFreeBuffer(out BufferEntry? entry)
    {
        foreach (var item in Buffers)
            if (item.Free)
            {
                entry = item;
                return true;
            }
        entry = null;
        Logger.Warn("Streaming buffer requested but all of them are occupied");
        return false;
    }

    public void Update()
    {
        AL.Source(SourceHandle, ALSourceb.Looping, false);

        AL.GetSource(SourceHandle, ALGetSourcei.SourceState, out int state);
        AL.GetSource(SourceHandle, ALGetSourcei.BuffersQueued, out int queued);
        AL.GetSource(SourceHandle, ALGetSourcei.BuffersProcessed, out int processed);

        if (Sound.State == SoundState.Paused && state == (int)ALSourceState.Playing)
            AL.SourcePause(SourceHandle);
        else if (Sound.State == SoundState.Playing && state == (int)ALSourceState.Paused)
            AL.SourcePlay(SourceHandle);

        // unqueue processed buffers
        if (processed > 0)
        {
            for (int p = 0; p < processed; p++)
            {
                var bufferHandle = AL.SourceUnqueueBuffer(SourceHandle);
                for (int i = 0; i < Buffers.Length; i++)
                    if (Buffers[i].Handle == bufferHandle)
                        Buffers[i].Free = true;
            }

            // check if end of file was reached right after all buffers are completely processed
            if (endReached)
            {
                AL.SourceRewind(SourceHandle);

                var sound = AudioObjects.Sources.GetSoundFor(SourceHandle);
                Reset();
                if (sound.Looping)
                    sound.State = SoundState.Stopped;
                return;
            }
        }

        // queue new buffers
        if (ShouldPlay)
        {
            if (state != (int)ALSourceState.Playing) //ShouldPlay is waar dus speel af, sukkel
                AL.SourcePlay(SourceHandle);

            int v = 0;
            while (queued++ < MaxBufferCount)
            {
                if (!TryRead(out var readAmount))
                {
                    endReached = true;
                    break;
                }

                if (!TryGetFreeBuffer(out var buffer))
                    continue;
                if (v++ == 0)
                    Array.Copy(rawOggBuffer, LastSamples, readAmount);
                else
                {
                    for (int i = 0; i < rawOggBuffer.Length; i++)
                        LastSamples[i] += rawOggBuffer[i];
                }

                buffer.Free = false;
                CastBuffer(rawOggBuffer, readBuffer, readAmount);
                AL.BufferData<short>(buffer.Handle, Format, readBuffer.AsSpan(0, readAmount), Raw.SampleRate);
                AL.SourceQueueBuffer(SourceHandle, buffer.Handle);
            }
        }
        else if (Sound.State == SoundState.Stopped && state != (int)ALSourceState.Stopped)
        {
            for (int p = 0; p < processed; p++)
                AL.SourceUnqueueBuffer(SourceHandle);

            foreach (var item in Buffers)
                item.Free = true;

            Sound.State = SoundState.Stopped;
            AL.SourceStop(SourceHandle);
            Reset();
        }
    }

    private static void CastBuffer(float[] inBuffer, short[] outBuffer, int length)
    {
        for (int i = 0; i < length; i++)
        {
            var temp = (int)(short.MaxValue * inBuffer[i]);

            if (temp > short.MaxValue)
                temp = short.MaxValue;
            else if (temp < short.MinValue)
                temp = short.MinValue;

            outBuffer[i] = (short)temp;
        }
    }

    private bool TryRead(out int read)
    {
        read = reader.ReadSamples(rawOggBuffer, 0, rawOggBuffer.Length);
        return read > 0;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        reader.Dispose();
        stream.Dispose();

        AL.SourceStop(SourceHandle);
        AL.GetSource(SourceHandle, ALGetSourcei.BuffersQueued, out int queued);
        if (queued > 0)
        {
            var buffers = new int[queued];
            AL.SourceUnqueueBuffers(SourceHandle, queued, buffers);
        }

        foreach (var b in Buffers)
            AL.DeleteBuffer(b.Handle);
    }
}