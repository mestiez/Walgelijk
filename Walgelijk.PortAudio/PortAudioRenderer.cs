using i3arnon.ConcurrentCollections;
using PortAudioSharp;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PA = PortAudioSharp.PortAudio;

namespace Walgelijk.PortAudio;

internal class FixedBufferCache : ConcurrentCache<FixedAudioData, float[]>
{
    public static FixedBufferCache Shared { get; } = new();

    protected override float[] CreateNew(FixedAudioData raw)
    {
        var data = new float[raw.SampleCount];

        // data is 16 bits per sample, interleaved
        // TODO resampling

        var p = 0;

        for (int i = 0; i < raw.Data.Length; i += 2)
        {
            var val = BitConverter.ToInt16(raw.Data.AsSpan(i, 2));
            data[p++] = (val / (float)short.MaxValue);
        }

        return data;
    }

    protected override void DisposeOf(float[] loaded)
    {

    }
}

internal interface IVoice
{
    public double Time { get; set; }
    public uint SampleIndex { get; set; }
    public Vector3 Position { get; set; }
    public bool IsVirtual { get; set; }

    public void Play();
    public void Pause();
    public void Resume();
    public void Stop();

    public void GetSamples(Span<float> frame);
}

internal class SharedVoice : IVoice
{
    public readonly Sound Sound;
    public readonly float[] Data;

    public SharedVoice(Sound sound)
    {
        Sound = sound;
        if (Sound.Data is FixedAudioData fixedAudioData)
            Data = FixedBufferCache.Shared.Load(fixedAudioData);
        else
            throw new Exception("Streaming source not yet supported"); // TODO
    }

    public double Time
    {
        get => SampleIndex * (PortAudioRenderer.SecondsPerSample / Sound.Data.ChannelCount);

        set
        {
            uint sampleIndex = uint.Clamp((uint)(value / (PortAudioRenderer.SecondsPerSample / Sound.Data.ChannelCount)), 0, (uint)Data.Length - 1);
            SampleIndex = sampleIndex;
        }
    }

    public uint SampleIndex { get; set; }
    public Vector3 Position { get; set; }
    public bool IsVirtual { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetSamples(Span<float> frame)
    {
        if (Sound.State is not SoundState.Playing)
            return;

        var trackVolume = Sound.Track?.Volume ?? 1;

        for (int i = 0; i < frame.Length; i++)
        {
            var nextSample = SampleIndex + 1;

            if (!Sound.Looping && nextSample >= Data.Length)
            {
                Stop();
                return;
            }

            SampleIndex = nextSample % (uint)Data.Length;
            var v = Data[SampleIndex] * trackVolume;

            frame[i] += v;
            if (Sound.Data.ChannelCount == 1) // mono, so we should copy our sample to the next channel
                frame[++i] += v;
        }
    }

    public void Pause()
    {
        Sound.State = SoundState.Paused;
    }

    public void Play()
    {
        Sound.State = SoundState.Playing;
    }

    public void Resume()
    {
        Sound.State = SoundState.Playing;
    }

    public void Stop()
    {
        Sound.State = SoundState.Stopped;
        SampleIndex = 0;
    }
}

internal class OneShotVoice : IVoice
{
    public readonly AudioData Data;
    public readonly SpatialParams? SpatialParams = new(1, float.PositiveInfinity, 1);
    public readonly float Pitch = 1;
    public readonly float Volume = 1;
    public readonly AudioTrack? Track;

    public double Time { get; set; }
    public uint SampleIndex { get; set; }
    public Vector3 Position { get; set; }
    public bool IsVirtual { get; set; }

    public OneShotVoice(Sound sound)
    {
        Data = sound.Data;
        SpatialParams = sound.SpatialParams;
        Pitch = sound.Pitch;
        Volume = sound.Volume;
        Track = sound.Track;
    }

    public void Play()
    {
    }

    public void Pause()
    {
    }

    public void Resume()
    {
    }

    public void Stop()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetSamples(Span<float> frame)
    {
    }
}

internal class SampleAggregator : IDisposable
{
    public readonly int MaxVoices, ProcessInterval;

    public float Volume { get; set; } = 1;
    public bool Muted { get; set; }
    public Vector3 ListenerPosition { get; set; }
    public (Vector3 Forward, Vector3 Up) ListenerOrientation { get; set; }
    public AudioDistanceModel DistanceModel { get; set; }

    public IEnumerable<IVoice> GetAll()
    {
        foreach (var item in sharedVoices.Values)
            yield return item; 
        foreach (var item in oneShotVoices)
            yield return item;
    }

    private readonly ConcurrentDictionary<Sound, SharedVoice> sharedVoices = [];
    private readonly ConcurrentHashSet<OneShotVoice> oneShotVoices = [];
    private Thread processThread;

    //private readonly SemaphoreSlim sharedvoiceSetLock = new(1);

    private bool isDisposed = false;

    public SampleAggregator(int maxVoices, int processInterval)
    {
        MaxVoices = maxVoices;
        ProcessInterval = processInterval;

        processThread = new(Process);
        processThread.IsBackground = true;
        processThread.Start();
    }

    private void Process(object? _)
    {
        var removeStack = new Stack<OneShotVoice>();

        while (!isDisposed)
        {
            Thread.Sleep(ProcessInterval);

            foreach (var s in oneShotVoices)
                if (s.Time > s.Data.Duration.TotalSeconds)
                    removeStack.Push(s);

            while (removeStack.TryPop(out var p))
                oneShotVoices.TryRemove(p);
        }
    }

    public void GetVoice(Sound sound, out SharedVoice voice)
    {
        if (sharedVoices.TryGetValue(sound, out voice))
            return;

        voice = new SharedVoice(sound);
        sharedVoices.AddOrSet(sound, voice);
    }

    public void CreateOneShot(Sound sound, out OneShotVoice voice)
    {
        voice = new OneShotVoice(sound);
        oneShotVoices.Add(voice);
    }

    public bool Contains(Sound sound) => sharedVoices.ContainsKey(sound);

    public void Dispose()
    {
        isDisposed = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetNextSamples(Span<float> buffer)
    {
        buffer.Clear();

        if (Muted)
            return;

        // TODO
        // - correctly handle state (paused, stop, play, etc.)
        // - correctly handle looping and pitch (for tracks too)

        // - resampling
        // - correctly deal with mono sources
        // - streaming

        // - voice cache array for faster enumeration

        foreach (var voice in sharedVoices.Values)
        {
            voice.GetSamples(buffer);
        }

        for (int i = 0; i < buffer.Length; i++)
            buffer[i] *= Volume;
    }
}

public class PortAudioRenderer : AudioRenderer
{
    public override float Volume { get => aggregator.Volume; set => aggregator.Volume = value; }
    public override bool Muted { get => aggregator.Muted; set => aggregator.Muted = value; }
    public override Vector3 ListenerPosition { get => aggregator.ListenerPosition; set => aggregator.ListenerPosition = value; }
    public override (Vector3 Forward, Vector3 Up) ListenerOrientation { get => aggregator.ListenerOrientation; set => aggregator.ListenerOrientation = value; }
    public override AudioDistanceModel DistanceModel { get => aggregator.DistanceModel; set => aggregator.DistanceModel = value; }

    public readonly int MaxVoices;

    private PortAudioSharp.Stream? stream;
    private int currentDeviceIndex = PA.NoDevice;
    private SampleAggregator aggregator;
    private float[] sampleBuffer;

    internal const int SampleRate = 44100;
    internal const int FramesPerBuffer = 256;
    internal const int ChannelCount = 2;
    internal const double SecondsPerSample = 1d / SampleRate;

    public PortAudioRenderer(int maxVoices = 512)
    {
        aggregator = new(MaxVoices, 100);
        sampleBuffer = new float[ChannelCount * FramesPerBuffer];

        PA.LoadNativeLibrary();
        PA.Initialize();

        ReinitialiseStream(PA.DefaultOutputDevice);
        MaxVoices = maxVoices;
    }

    private void ReinitialiseStream(int deviceIndex)
    {
        if (stream != null)
        {
            stream.Close();
            stream.Dispose();
        }

        var outParams = new StreamParameters
        {
            device = deviceIndex,
            channelCount = ChannelCount,
            sampleFormat = SampleFormat.Float32,
            suggestedLatency = PA.GetDeviceInfo(deviceIndex).defaultLowOutputLatency
        };

        stream = new PortAudioSharp.Stream(null, outParams, SampleRate, FramesPerBuffer, default, OnPaCallback, null);

        stream.Start();
    }

    private StreamCallbackResult OnPaCallback(nint input, nint output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, nint userDataPtr)
    {
        /* TODO 
        /* - Remember: this function could be invoked at ANY TIME, it's likely to be on a different thread. Offload most of the work away from this function.
        /* - Make SoundProcessor object to keep track of currently playing sounds
        /* - Loop through all processors and get their samples
        /* - 3D audio, volume, attenuation, mute state, pitch, etc.
        /* - BONUS: effects? convolve? SIMD? HDR!!!
        */

        aggregator.GetNextSamples(sampleBuffer); // todo do this somewhere else, read results in this function
        Marshal.Copy(sampleBuffer, 0, output, sampleBuffer.Length);

        return StreamCallbackResult.Continue;
    }

    public override void DisposeOf(AudioData audioData)
    {
    }

    public override void DisposeOf(Sound sound)
    {
    }

    public override IEnumerable<string> EnumerateAvailableAudioDevices()
    {
        for (int i = 0; i < PA.DeviceCount; i++)
        {
            var info = PA.GetDeviceInfo(i);
            yield return info.name;
        }
    }

    public override string GetCurrentAudioDevice()
    {
        var info = PA.GetDeviceInfo(currentDeviceIndex);
        return info.name;
    }

    public override int GetCurrentSamples(Sound sound, Span<float> arr)
    {
        return 0;
    }

    public override float GetTime(Sound sound)
    {
        aggregator.GetVoice(sound, out var voice);
        return (float)voice.Time;
    }

    public override bool IsPlaying(Sound sound)
    {
        return aggregator.Contains(sound) && sound.State == SoundState.Playing;
    }

    public override FixedAudioData LoadSound(string path)
    {
        throw new NotImplementedException();
    }

    public override StreamAudioData LoadStream(string path)
    {
        throw new NotImplementedException();
    }

    public override void Pause(Sound sound)
    {
        aggregator.GetVoice(sound, out var voice);
        voice.Pause();
    }

    public override void PauseAll()
    {
    }

    public override void PauseAll(AudioTrack track)
    {
    }

    public override void Play(Sound sound, float volume = 1)
    {
        aggregator.GetVoice(sound, out var voice);
        voice.Play();
    }

    public override void Play(Sound sound, Vector3 worldPosition, float volume = 1)
    {
        aggregator.GetVoice(sound, out var voice);
        voice.Position = worldPosition;
        voice.Play();
    }

    public override void PlayOnce(Sound sound, float volume = 1, float pitch = 1, AudioTrack? track = null)
    {
    }

    public override void PlayOnce(Sound sound, Vector3 worldPosition, float volume = 1, float pitch = 1, AudioTrack? track = null)
    {
    }

    public override void Process(float dt)
    {
    }

    public override void Release()
    {
        try
        {
            stream?.Dispose();
            PA.Terminate();
            aggregator.Dispose();
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    public override void ResumeAll()
    {
    }

    public override void ResumeAll(AudioTrack track)
    {
    }

    public override void SetAudioDevice(string device)
    {
        for (int i = 0; i < PA.DeviceCount; i++)
        {
            var info = PA.GetDeviceInfo(i);
            if (info.name.Equals(device))
            {
                ReinitialiseStream(i);
                return;
            }
        }

        throw new Exception($"Could not find audio device matching {device}");
    }

    public override void SetPosition(Sound sound, Vector3 worldPosition)
    {
        aggregator.GetVoice(sound, out var voice);
        voice.Position = worldPosition;
    }

    public override void SetTime(Sound sound, float seconds)
    {
        aggregator.GetVoice(sound, out var voice);
        voice.Time = seconds;
    }

    public override void Stop(Sound sound)
    {
        aggregator.GetVoice(sound, out var voice);
        voice.Stop();
    }

    public override void StopAll()
    {
    }

    public override void StopAll(AudioTrack track)
    {
    }

    public override void UpdateTracks()
    {
    }
}
