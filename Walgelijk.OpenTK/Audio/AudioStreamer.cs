using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Walgelijk.OpenTK;

public class AudioStreamer : IDisposable
{
    public const int BufferSize = 2048;
    public const int MaxBufferCount = 4;

    public readonly SourceHandle Source;
    public readonly Sound Sound;
    public readonly StreamAudioData AudioData;
    public readonly Dictionary<BufferHandle, BufferEntry> Buffers = [];

    public ALFormat Format => AudioData.ChannelCount == 1 ? ALFormat.MonoFloat32Ext : ALFormat.StereoFloat32Ext;

    public TimeSpan CurrentTime
    {
        get
        {
            return stream.TimePosition; 
            //AL.GetSource(Source, ALGetSourcei.SampleOffset, out int samplesPlayedInThisBuffer);
            //ALUtils.CheckError();
            //var total = samplesPlayedInThisBuffer + processedSamples;
            //return TimeSpan.FromSeconds((total / (float)AudioData.SampleRate / AudioData.ChannelCount) %
            //                            (float)Sound.Data.Duration.TotalSeconds);
        }

        set =>
            stream.TimePosition = value > TimeSpan.Zero
                ? (value < AudioData.Duration ? value : AudioData.Duration)
                : TimeSpan.Zero;
    }

    private readonly float[] rawBuffer = new float[BufferSize];
    private readonly int[] bufferHandles = new int[MaxBufferCount];
    private readonly Thread thread;
    private readonly CircularBuffer<float> recentlyPlayed = new(1024);

    private int processedSamples = 0;

    private IAudioStream stream;
    private bool endReached;
    private volatile bool monitorFlag = true;

    public AudioStreamer(SourceHandle sourceHandle, Sound sound, StreamAudioData raw)
    {
        AudioData = raw;
        Source = sourceHandle;
        Sound = sound;

        stream = AudioData.InputSourceFactory();

        for (int i = 0; i < MaxBufferCount; i++)
        {
            var a = new BufferEntry(AL.GenBuffer());
            ALUtils.CheckError();
            Buffers.Add(a.Handle, a);
            bufferHandles[i] = a.Handle;
        }

        thread = new Thread(ThreadLoop);
        thread.IsBackground = true;
        thread.Start();
    }

    private void FillQueue()
    {
        foreach (var b in Buffers.Values)
            FillBuffer(b);
    }

    private bool FillBuffer(BufferEntry buffer)
    {
        if (ReadSamples(out var readAmount))
        {
            Array.Copy(rawBuffer, buffer.Data, BufferSize);

            AL.BufferData<float>(buffer.Handle, Format, rawBuffer.AsSpan(0, readAmount), AudioData.SampleRate);
            ALUtils.CheckError();
            AL.SourceQueueBuffer(Source, buffer.Handle);
            ALUtils.CheckError();

            lock (recentlyPlayed)
            {
                for (int i = 0; i < readAmount; i++)
                    recentlyPlayed.Write(rawBuffer[i]);
            }

            return true;
        }

        return false;
    }

    private void ThreadLoop()
    {
        FillQueue();

        if (Sound.State == SoundState.Playing)
            AL.SourcePlay(Source);
        ALUtils.CheckError();

        while (monitorFlag)
        {
            Thread.Sleep(8);

            if (!AL.IsSource(Source))
            {
                this.Dispose();
                break;
            }

            var state = Source.GetALState();
            ALUtils.CheckError();
            var processed = AL.GetSource(Source, ALGetSourcei.BuffersProcessed);
            ALUtils.CheckError();

            switch (Sound.State)
            {
                case SoundState.Playing:
                    // if we are requested to play, didnt reach the end yet, and the source isnt playing: play the source
                    if (!endReached && state is ALSourceState.Stopped or ALSourceState.Initial or ALSourceState.Paused)
                        AL.SourcePlay(Source);
                    ALUtils.CheckError();
                    processed = AL.GetSource(Source, ALGetSourcei.BuffersProcessed);
                    ALUtils.CheckError();
                    break;
                case SoundState.Paused:
                    // if we are requested to pause and the source is still playing, pause
                    if (state == ALSourceState.Playing)
                        AL.SourcePause(Source);
                    ALUtils.CheckError();
                    continue;
                case SoundState.Stopped:
                    // if we are requested to stop and the source isnt stopped, stop
                    if (state is ALSourceState.Playing or ALSourceState.Paused)
                    {
                        AL.SourceStop(Source);
                        ALUtils.CheckError();
                        processed = AL.GetSource(Source, ALGetSourcei.BuffersProcessed);
                        ALUtils.CheckError();

                        for (int i = 0; i < processed; i++)
                        {
                            AL.SourceUnqueueBuffer(Source);
                            ALUtils.CheckError();
                        }

                        //AL.SourceRewind(Source);
                        ALUtils.CheckError();

                        stream?.Dispose();

                        endReached = false;
                        processedSamples = 0;
                        stream = AudioData.InputSourceFactory();

                        FillQueue();
                    }
                    continue;
            }

            while (processed > 0)
            {
                int bufferHandle = AL.SourceUnqueueBuffer(Source);
                ALUtils.CheckError();

                if (Sound.State is SoundState.Playing && Buffers.TryGetValue(bufferHandle, out var buffer))
                    if (!FillBuffer(buffer))
                    {
                        endReached = true;
                        break;
                    }

                processed--;
                processedSamples += BufferSize;
            }

            if (endReached && !Sound.Looping)
                Sound.State = SoundState.Stopped;
        }
    }

    private bool ReadSamples(out int read)
    {
        read = stream.ReadSamples(rawBuffer);
        if (Sound.Looping)
        {
            // if the file is set to loop, just start from the beginning again
            if (read == 0)
            {
                stream.Position = 0;
                return ReadSamples(out read);
            }
        }

        return read > 0;
    }

    public void Dispose()
    {
        monitorFlag = false;
        GC.SuppressFinalize(this);

        AL.SourceStop(Source);
        AL.Source(Source, ALSourcei.Buffer, 0);

        foreach (var b in Buffers.Keys)
            AL.DeleteBuffer(b.Handle);

        stream.Dispose();
    }

    public IEnumerable<float> TakeLastPlayed(int count = 1024)
    {
        lock (recentlyPlayed)
        {
            for (int i = 0; i < count; i++)
                yield return recentlyPlayed.Read();
        }
    }

    public class BufferEntry(BufferHandle bufferHandle)
    {
        public readonly BufferHandle Handle = bufferHandle;
        public float[] Data = new float[BufferSize];
        public override string ToString() => Handle.ToString();
    }
}