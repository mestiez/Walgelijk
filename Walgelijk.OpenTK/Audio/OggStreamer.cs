using NVorbis;
using OpenTK.Audio.OpenAL;
using System;
using System.IO;
using System.Threading;

namespace Walgelijk.OpenTK;

public class OggStreamer : IDisposable
{
    public const int BufferSize = 1024;
    public const int MaxBufferCount = 16;

    public ALFormat Format => Raw.ChannelCount == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
    public readonly SourceHandle SourceHandle;
    public readonly Sound Sound;
    public readonly StreamAudioData Raw;
    public readonly BufferEntry[] Buffers = new BufferEntry[MaxBufferCount];

    private int lastProcessedSampleCount = 0;
    private int processedSamples = 0;

    public TimeSpan CurrentTime
    {
        get => TimeSpan.FromSeconds(processedSamples / (float)reader.SampleRate / Raw.ChannelCount);

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
    private readonly Thread monitorThread;
    private volatile bool monitorFlag = true;

    public BufferHandle? CurrentPlayingBuffer;
    public float[] LastSamples = new float[BufferSize];

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
        Array.Clear(LastSamples);

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
        CastBuffer(rawOggBuffer, readBuffer, readAmount);
        AL.BufferData<short>(buffer.Handle, Format, readBuffer.AsSpan(0, readAmount), Raw.SampleRate);
        AL.SourceQueueBuffer(SourceHandle, buffer.Handle);
    }

    public void PreFill()
    {
        int queued = 0;
        while (queued++ < MaxBufferCount && TryRead(out var readAmount))
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
            Thread.Sleep(32);
        }
    }

    public void Update()
    {
        // deal with looping ourselves
        AL.Source(SourceHandle, ALSourceb.Looping, false);

        var state = AL.GetSourceState(SourceHandle);
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
            processedSamples = lastProcessedSampleCount + samplesPlayedInThisBuffer;
        }

        // set last sample buffer
        AL.GetSource(SourceHandle, ALGetSourcei.Buffer, out int currentBufferID);
        if (currentBufferID != CurrentPlayingBuffer)
        {
            CurrentPlayingBuffer = currentBufferID;
            for (int i = 0; i < Buffers.Length; i++)
                if (Buffers[i].Handle == currentBufferID)
                {
                    Array.Copy(Buffers[i].Data, LastSamples, BufferSize);
                    break;
                }
        }

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
                AL.SourceStop(SourceHandle);
                var sound = AudioObjects.Sources.GetSoundFor(SourceHandle);
                Reset();
                if (!sound.Looping)
                    sound.State = SoundState.Stopped;
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
        monitorFlag = false;
        monitorThread.Join();

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