using PortAudioSharp;
using System.Numerics;
using PA = PortAudioSharp.PortAudio;

namespace Walgelijk.PortAudio;

public class PortAudioRenderer : AudioRenderer
{
    public override float Volume { get; set; }
    public override bool Muted { get; set; }
    public override Vector3 ListenerPosition { get; set; }
    public override (Vector3 Forward, Vector3 Up) ListenerOrientation { get; set; }
    public override AudioDistanceModel DistanceModel { get; set; }

    private PortAudioSharp.Stream? stream;

    private const int SampleRate = 44100;
    private const int FramesPerBuffer = 256;
    private const int ChannelCount = 2;

    private const double SecondsPerSample = 1d / SampleRate;

    public PortAudioRenderer()
    {
        PA.LoadNativeLibrary();
        PA.Initialize();

        ReinitialiseStream(PA.DefaultOutputDevice);
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

        stream = new PortAudioSharp.Stream(null, outParams, SampleRate, FramesPerBuffer, StreamFlags.ClipOff, OnPaCallback, null);

        stream.Start();
    }

    static double t = 0;

    private static StreamCallbackResult OnPaCallback(nint input, nint output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, nint userDataPtr)
    {
        double frq = double.Lerp(100, 750, Utilities.Hash((float)(-0.0253 * double.Floor(timeInfo.currentTime * 10.63435))));

        unsafe
        {
            var outData = (float*)output;

            /* TODO 
            /* - Remember: this function could be invoked at ANY TIME, it's likely to be on a different thread. Offload most of the work away from this function.
            /* - Make SoundProcessor object to keep track of currently playing sounds
            /* - Loop through all processors and get their samples
            /* - 3D audio, volume, attenuation, mute state, pitch, etc.
            /* - BONUS: effects? convolve? SIMD? 
            */

            uint i = 0;
            for (uint f = 0; f < frameCount; f++)
            {
                var val = (float)(double.Sin(t) * 0.5);

                for (int c = 0; c < ChannelCount; c++)
                    outData[i++] = val;

                t += SecondsPerSample * frq * double.Tau;
            }
        }

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
        var info = PA.GetDeviceInfo(PA.DefaultOutputDevice);
        return info.name;
    }

    public override int GetCurrentSamples(Sound sound, Span<float> arr)
    {
        return 0;
    }

    public override float GetTime(Sound sound)
    {
        return 0;
    }

    public override bool IsPlaying(Sound sound)
    {
        return false;
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
    }

    public override void PauseAll()
    {
    }

    public override void PauseAll(AudioTrack track)
    {
    }

    public override void Play(Sound sound, float volume = 1)
    {
    }

    public override void Play(Sound sound, Vector3 worldPosition, float volume = 1)
    {
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
        stream?.Dispose();
        PA.Terminate();
    }

    public override void ResumeAll()
    {
    }

    public override void ResumeAll(AudioTrack track)
    {
    }

    public override void SetAudioDevice(string device)
    {
    }

    public override void SetPosition(Sound sound, Vector3 worldPosition)
    {
    }

    public override void SetTime(Sound sound, float seconds)
    {
    }

    public override void Stop(Sound sound)
    {
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
