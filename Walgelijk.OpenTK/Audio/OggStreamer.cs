//#define USE_FLOAT_EXT

using NVorbis;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Walgelijk.OpenTK;

public class OggStreamer : IDisposable
{
    public const int BufferSize = 1024;
    public const int MaxBufferCount = 16;

#if USE_FLOAT_EXT
    public FloatBufferFormat Format => Raw.ChannelCount == 1 ? FloatBufferFormat.Mono : FloatBufferFormat.Stereo;
#else
    public ALFormat Format => Raw.ChannelCount == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
    private readonly short[] shortDataBuffer = new short[BufferSize];
#endif

    public readonly SourceHandle SourceHandle;
    public readonly Sound Sound;
    public readonly StreamAudioData Raw;
    public readonly BufferEntry[] Buffers = new BufferEntry[MaxBufferCount];

    private int lastProcessedSampleCount = 0;
    private int processedSamples = 0;

    public TimeSpan CurrentTime
    {
        get => TimeSpan.FromSeconds((processedSamples / (float)reader.SampleRate / Raw.ChannelCount) % (float)Sound.Data.Duration.TotalSeconds);

        set
        {
            reader.TimePosition = value > TimeSpan.Zero ? (value < reader.TotalTime ? value : reader.TotalTime) : TimeSpan.Zero;
        }
    }

    private FileStream stream;
    private VorbisReader reader;
    private bool endReached;
    private readonly float[] rawOggBuffer = new float[BufferSize];
    private readonly Thread monitorThread;
    private volatile bool monitorFlag = true;

    public BufferHandle? CurrentPlayingBuffer;

    private readonly ConcurrentStack<float> playedSamplesBacklog = new();

    public IEnumerable<float> TakeLastPlayed(int count = 1024)
    {
        for (int i = 0; i < count; i++)
            if (playedSamplesBacklog.TryPop(out var f))
                yield return f;
            else
                yield break;
    }

    public class BufferEntry
    {
        public readonly BufferHandle Handle;
        public bool Free;
        public float[] Data = new float[BufferSize];

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

        monitorThread = new Thread(MonitorLoop);
        monitorThread.Start();

    }

    public void Reset()
    {
        AL.GetSource(SourceHandle, ALGetSourcei.BuffersProcessed, out int processed);

        for (int p = 0; p < processed; p++)
            AL.SourceUnqueueBuffer(SourceHandle);

        foreach (var item in Buffers)
            item.Free = true;

        endReached = false;
        stream?.Dispose();
        reader?.Dispose();

        stream = new FileStream(Raw.File.FullName, FileMode.Open, FileAccess.Read);
        reader = new VorbisReader(stream, true);
        playedSamplesBacklog.Clear();

        lastProcessedSampleCount = 0;
        processedSamples = 0;
        CurrentPlayingBuffer = null;

        AL.SourceRewind(SourceHandle);

        PreFill();
    }

    private void FillBuffer(BufferEntry buffer, int readAmount)
    {
        buffer.Free = false;
        Array.Copy(rawOggBuffer, buffer.Data, BufferSize);

#if USE_FLOAT_EXT
        AL.EXTFloat32.BufferData(buffer.Handle, Format, buffer.Data.AsSpan(0, readAmount), Raw.SampleRate);
        if (!AL.EXTFloat32.IsExtensionPresent())
            throw new Exception($"The provided OpenAL distribution is missing the \"{AL.EXTFloat32.ExtensionName}\" extension");
#else
        for (int i = 0; i < readAmount; i++)
        {
            var v = (short)Utilities.MapRange(-1, 1, short.MinValue, short.MaxValue, buffer.Data[i]);
            shortDataBuffer[i] = v;
        }
        AL.BufferData<short>(buffer.Handle, Format, shortDataBuffer.AsSpan(0, readAmount), Raw.SampleRate);
#endif

        AL.SourceQueueBuffer(SourceHandle, buffer.Handle);

        for (int i = 0; i < readAmount; i++)
            playedSamplesBacklog.Push(rawOggBuffer[i]);
    }

    public void PreFill(int max = MaxBufferCount)
    {
        int queued = 0;
        while (queued++ < max && TryRead(out var readAmount))
        {
            if (!TryGetFreeBuffer(out var buffer))
                continue;
            FillBuffer(buffer, readAmount);
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

    private void MonitorLoop()
    {
        while (monitorFlag)
        {
            Update();
            Thread.Sleep(16);
        }
    }

    public void Update()
    {
        // deal with looping ourselves
        AL.Source(SourceHandle, ALSourceb.Looping, false);

        var state = SourceHandle.GetSourceState();
        AL.GetSource(SourceHandle, ALGetSourcei.BuffersQueued, out int queued);
        AL.GetSource(SourceHandle, ALGetSourcei.BuffersProcessed, out int processed);

        // sync sound state and source state
        switch (Sound.State)
        {
            case SoundState.Playing:
                if (!endReached && state != ALSourceState.Playing)
                    AL.SourcePlay(SourceHandle);
                break;
            case SoundState.Paused:
                if (state == ALSourceState.Playing)
                    AL.SourcePause(SourceHandle);
                break;
            case SoundState.Stopped:
                if (state is ALSourceState.Playing or ALSourceState.Paused)
                {
                    AL.SourceStop(SourceHandle);
                    Reset();
                }
                break;
        }

        // calculate current time offset
        if (Sound.State is SoundState.Stopped or SoundState.Idle)
            lastProcessedSampleCount = 0;
        else
        {
            AL.GetSource(SourceHandle, ALGetSourcei.SampleOffset, out int samplesPlayedInThisBuffer);
            lastProcessedSampleCount += processed * BufferSize;
            processedSamples = lastProcessedSampleCount;//+ samplesPlayedInThisBuffer;
        }

        // read processed buffer count again in case any of them were discarded
        AL.GetSource(SourceHandle, ALGetSourcei.BuffersProcessed, out processed);

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
        }
        // check if end of file was reached right after there are no more buffers to process
        else if (queued == 0 && endReached)
        {
            var sound = AudioObjects.Sources.GetSoundFor(SourceHandle);
            if (!sound.Looping)
            {
                AL.SourceStop(SourceHandle);
                sound.State = SoundState.Stopped;
                Reset();
                return;
            }
        }

        // queue new buffers
        if (Sound.State is SoundState.Playing && !endReached)
        {
            while (queued++ < MaxBufferCount)
            {
                if (!TryRead(out var readAmount))
                {
                    endReached = true;
                    break;
                }

                if (TryGetFreeBuffer(out var buffer))
                    FillBuffer(buffer, readAmount);
            }
        }
    }

    private bool TryRead(out int read)
    {
        read = reader.ReadSamples(rawOggBuffer, 0, rawOggBuffer.Length);
        if (Sound.Looping)
        {
            if (read == 0)
            {
                reader.SamplePosition = 0;
                return TryRead(out read);
            }
        }

        return read > 0;
    }

    public void Dispose()
    {
        monitorFlag = false;
        monitorThread.Join();

        GC.SuppressFinalize(this);
        reader.Dispose();
        stream.Dispose();

        AL.SourceStop(SourceHandle);
        AL.Source(SourceHandle, ALSourcei.Buffer, 0);

        foreach (var b in Buffers)
            AL.DeleteBuffer(b.Handle);
    }
}