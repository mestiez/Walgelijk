using NVorbis;
using OpenTK.Audio.OpenAL;
using System;
using System.IO;

namespace Walgelijk.OpenTK;

public class OggStreamer : IDisposable
{
    public const int BufferSize = 1024;
    public const int MaxBufferCount = 32;

    public ALFormat Format => Raw.ChannelCount == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
    public readonly SourceHandle SourceHandle;
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

    public OggStreamer(SourceHandle sourceHandle, StreamAudioData raw)
    {
        Raw = raw;
        SourceHandle = sourceHandle;

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

        if ((ALSourceState)state == ALSourceState.Stopped)
        {
            foreach (var item in Buffers)
                item.Free = true;

            for (int p = 0; p < processed; p++)
                AL.SourceUnqueueBuffer(SourceHandle);

            AL.SourceRewind(SourceHandle);
            Reset();
            return;
        }

        // unqueue processed buffers
        if (processed > 0)
        {
            for (int p = 0; p < processed; p++)
            {
                var bufferHandle = AL.SourceUnqueueBuffer(SourceHandle);
                for (int i = 0; i < Buffers.Length; i++)
                    if (Buffers[i].Handle == bufferHandle)
                    {
                        Buffers[i].Free = true;
                    }
            }

            // check if end of file was reached right after all buffers are completely processed
            if (endReached)
            {
                AL.SourceRewind(SourceHandle);

                var sound = AudioObjects.Sources.GetSoundFor(SourceHandle);
                if (sound.Looping)
                {
                    Reset();
                    AL.SourcePlay(SourceHandle);
                }
                return;
            }
        }

        // queue new buffers
        if ((ALSourceState)state is ALSourceState.Playing or ALSourceState.Initial)
            while (queued++ < MaxBufferCount)
            {
                if (!TryRead(out var readAmount))
                {
                    endReached = true;
                    break;
                }

                if (!TryGetFreeBuffer(out var buffer))
                    continue;
                buffer.Free = false;
                CastBuffer(rawOggBuffer, readBuffer, readAmount);
                AL.BufferData<short>(buffer.Handle, Format, readBuffer.AsSpan(0, readAmount), Raw.SampleRate);
                AL.SourceQueueBuffer(SourceHandle, buffer.Handle);
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