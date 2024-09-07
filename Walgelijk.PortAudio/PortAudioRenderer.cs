using PortAudioSharp;
using System.Numerics;
using System.Runtime.InteropServices;
using PA = PortAudioSharp.PortAudio;

namespace Walgelijk.PortAudio;

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
    private readonly SampleAggregator aggregator;
    private readonly float[] sampleBuffer;
    private (string, int)[] devices = [];
    private readonly SemaphoreSlim usingStreamLock = new(1);

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

        var deviceInfo = PA.GetDeviceInfo(PA.DefaultOutputDevice);

        var d = new HashSet<(string, int)>();
        for (int i = 0; i < PA.DeviceCount; i++)
        {
            var info = PA.GetDeviceInfo(i);
            if (info.maxOutputChannels >= 2 && info.hostApi == deviceInfo.hostApi && info.defaultSampleRate >= SampleRate)
                if (!d.Any(a => a.Item1 == info.name))
                    d.Add((info.name, i));
        }
        devices = [.. d];

        ReinitialiseStream(PA.DefaultOutputDevice);
        MaxVoices = maxVoices;
    }

    private void ReinitialiseStream(int deviceIndex)
    {
        usingStreamLock.Wait();
        try
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }

            var deviceInfo = PA.GetDeviceInfo(deviceIndex);

            var outParams = new StreamParameters
            {
                device = deviceIndex,
                channelCount = ChannelCount,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = deviceInfo.defaultLowOutputLatency
            };

            stream = new PortAudioSharp.Stream(null, outParams, SampleRate, FramesPerBuffer, default, OnPaCallback, null);
            currentDeviceIndex = deviceIndex;
            stream.Start();
        }
        finally
        {
            usingStreamLock.Release();
        }
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
        usingStreamLock.Wait();
        try
        {
            aggregator.GetNextSamples(sampleBuffer); // todo do this somewhere else, read results in this function
            Marshal.Copy(sampleBuffer, 0, output, sampleBuffer.Length);

            return StreamCallbackResult.Continue;
        }
        finally
        {
            usingStreamLock.Release();

        }
    }

    public override IEnumerable<string> EnumerateAvailableAudioDevices() => devices.Select(d => d.Item1);

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
        aggregator.GetShared(sound, out var voice);
        return (float)voice.Time;
    }

    public override bool IsPlaying(Sound sound)
    {
        return aggregator.Contains(sound) && sound.State == SoundState.Playing;
    }

    public override void Pause(Sound sound)
    {
        aggregator.GetShared(sound, out var voice);
        voice.Pause();
    }

    public override void PauseAll()
    {
        foreach (var item in aggregator.GetAll())
            item.Pause();
    }

    public override void PauseAll(AudioTrack track)
    {
        foreach (var item in aggregator.GetAll(track))
            item.Pause();
    }

    public override void Play(Sound sound, float volume = 1)
    {
        aggregator.GetShared(sound, out var voice);
        voice.Volume = volume;
        voice.Play();
    }

    public override void Play(Sound sound, Vector3 worldPosition, float volume = 1)
    {
        aggregator.GetShared(sound, out var voice);

        voice.Position = worldPosition;
        voice.Volume = volume;

        voice.Play();
    }

    public override void PlayOnce(Sound sound, float volume = 1, float pitch = 1, AudioTrack? track = null)
    {
        aggregator.GetOneShot(sound, out var voice);

        voice.Pitch = pitch;
        voice.Volume = volume;

        voice.Play();
    }

    public override void PlayOnce(Sound sound, Vector3 worldPosition, float volume = 1, float pitch = 1, AudioTrack? track = null)
    {
        aggregator.GetOneShot(sound, out var voice);

        voice.Position = worldPosition;
        voice.Pitch = pitch;
        voice.Volume = volume;

        voice.Play();
    }

    public override void Release()
    {
        try
        {
            stream?.Dispose();
            PA.Terminate();
            aggregator.Dispose();
            usingStreamLock.Dispose();
            FixedBufferCache.Shared.UnloadAll();
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    public override void ResumeAll()
    {
        foreach (var item in aggregator.GetAll())
            if (item.State == SoundState.Paused)
                item.Play();
    }

    public override void ResumeAll(AudioTrack track)
    {
        foreach (var item in aggregator.GetAll(track))
            if (item.State == SoundState.Paused)
                item.Play();
    }

    public override void SetAudioDevice(string device)
    {
        foreach (var item in devices)
        {
            if (item.Item1 == device)
            {
                ReinitialiseStream(item.Item2);
                return;
            }
        }

        throw new Exception($"Could not find audio device matching {device}");
    }

    public override void SetPosition(Sound sound, Vector3 worldPosition)
    {
        aggregator.GetShared(sound, out var voice);
        voice.Position = worldPosition;
    }

    public override void SetTime(Sound sound, float seconds)
    {
        aggregator.GetShared(sound, out var voice);
        voice.Time = seconds;
    }

    public override void Stop(Sound sound)
    {
        aggregator.GetShared(sound, out var voice);
        voice.Stop();
    }

    public override void StopAll()
    {
        foreach (var item in aggregator.GetAll())
            if (item.State == SoundState.Paused)
                item.Stop();
    }

    public override void StopAll(AudioTrack track)
    {
        foreach (var item in aggregator.GetAll(track))
            if (item.State == SoundState.Paused)
                item.Stop();
    }

    public override void UpdateTracks()
    {
        // we do nothing lol
    }

    public override void Process(float dt)
    {
        // we do nothing lol
    }

    public override void DisposeOf(AudioData audioData)
    {
        switch (audioData)
        {
            case FixedAudioData d:
                FixedBufferCache.Shared.Unload(d);
                break;
        }
    }

    public override void DisposeOf(Sound sound)
    {
        // we do nothing lol
    }

}
