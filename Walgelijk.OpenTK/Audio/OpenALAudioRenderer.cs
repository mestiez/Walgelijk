using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Walgelijk.OpenTK;

public class OpenALAudioRenderer : AudioRenderer
{
    public readonly int MaxTempSourceCount = 128;

    private ALDevice device;
    private ALContext context;
    private bool canPlayAudio = false;
    private bool canEnumerateDevices = false;
    private readonly TemporarySourcePool temporarySources;

    private readonly Dictionary<AudioTrack, HashSet<Sound>> tracks = new();
    private readonly Dictionary<Sound, AudioTrack?> trackBySound = new();
    private readonly TemporarySource[] temporarySourceBuffer;

    public IEnumerable<TemporarySource> TemporarySourceBuffer => temporarySources.GetAllInUse();
    public int CreatedTemporarySourceCount => temporarySources.CreatedAmount;

    public override float Volume
    {
        get => AL.GetListener(ALListenerf.Gain);

        set => AL.Listener(ALListenerf.Gain, value);
    }

    public override bool Muted
    {
        get => Volume <= float.Epsilon;
        set => Volume = 0;
    }

    public override Vector3 ListenerPosition
    {
        get
        {
            AL.GetListener(ALListener3f.Position, out float x, out float y, out float z);
            return new Vector3(x, y, z);
        }

        set => AL.Listener(ALListener3f.Position, value.X, value.Y, value.Z);
    }

    public override AudioDistanceModel DistanceModel
    {
        get => AL.GetDistanceModel() switch
        {
            ALDistanceModel.LinearDistance => AudioDistanceModel.Linear,
            _ => AudioDistanceModel.InverseSquare,
        };

        set => AL.DistanceModel(value switch
        {
            AudioDistanceModel.Linear => ALDistanceModel.LinearDistance,
            _ => ALDistanceModel.InverseDistance,
        });
    }

    public override (Vector3 Forward, Vector3 Up) ListenerOrientation
    {
        get
        {
            AL.GetListener(ALListenerfv.Orientation, out var at, out var up);
            return (new Vector3(at.X, at.Y, at.Z), new Vector3(up.X, up.Y, up.Z));
        }

        set
        {
            var at = new global::OpenTK.Mathematics.Vector3(value.Forward.X, value.Forward.Y, value.Forward.Z);
            var up = new global::OpenTK.Mathematics.Vector3(value.Up.X, value.Up.Y, value.Up.Z);
            AL.Listener(ALListenerfv.Orientation, ref at, ref up);
        }
    }

    public OpenALAudioRenderer(int maxTempSourceCount = 128)
    {
        //ALC.RegisterOpenALResolver();
        AL.RegisterOpenALResolver();

        MaxTempSourceCount = maxTempSourceCount;
        temporarySources = new(MaxTempSourceCount);
        temporarySourceBuffer = new TemporarySource[MaxTempSourceCount];

        Initialise();
    }

    private void Initialise(string? deviceName = null)
    {
        canPlayAudio = false;
        device = ALC.OpenDevice(deviceName);

        if (device == ALDevice.Null)
            Logger.Error(
                deviceName == null ? "No audio device could be found" : "The requested audio device could not be found",
                nameof(OpenALAudioRenderer));
        context = ALC.CreateContext(device, new ALContextAttributes());

        if (context == ALContext.Null)
            Logger.Error("No audio context could be created", nameof(OpenALAudioRenderer));

        bool couldSetContext = ALC.MakeContextCurrent(context);

        canPlayAudio = device != ALDevice.Null && context != ALContext.Null && couldSetContext;

        if (!couldSetContext)
            Logger.Error("The audio context could not be set", nameof(OpenALAudioRenderer));

        if (!canPlayAudio)
            Logger.Error("Failed to initialise the audio renderer", nameof(OpenALAudioRenderer));

        canEnumerateDevices = ALC.IsEnumerationExtensionPresent(device);

        if (!AL.EXTFloat32.IsExtensionPresent())
            throw new Exception("AL Float32 extension not available");

    }

    private void UpdateIfRequired(Sound sound, out int source)
    {
        // TODO this is Fucked Up
        source = -1;
        try
        {
            source = AudioObjects.Sources.Load(sound);
            if (!sound.RequiresUpdate && !(sound.Track?.RequiresUpdate ?? false))
                return;

            sound.RequiresUpdate = false;

            AL.Source(source, ALSourceb.Looping, sound.Data is not StreamAudioData && sound.Looping);
            AL.Source(source, ALSourcef.Pitch, sound.Pitch * (sound.Track?.Pitch ?? 1));
            AL.Source(source, ALSourcef.Gain,
                (sound.Track?.Muted ?? false) ? 0 : (sound.Volume * (sound.Track?.Volume ?? 1)));

            ApplySpatialParams(source, sound);
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
        finally
        {

        }
    }

    private void ApplySpatialParams(int source, Sound sound)
    {
        AL.Source(source, ALSourceb.SourceRelative, !sound.SpatialParams.HasValue);

        if (sound.SpatialParams.HasValue)
        {
            AL.Source(source, ALSourcef.MaxDistance, sound.SpatialParams.Value.MaxDistance);
            AL.Source(source, ALSourcef.ReferenceDistance, sound.SpatialParams.Value.ReferenceDistance);
            AL.Source(source, ALSourcef.RolloffFactor, sound.SpatialParams.Value.RolloffFactor);
        }
        else
        {
            AL.Source(source, ALSourcef.MaxDistance, 0);
            AL.Source(source, ALSourcef.ReferenceDistance, 0);
            AL.Source(source, ALSourcef.RolloffFactor, 0);
        }
    }

    public override void Pause(Sound sound)
    {
        if (!canPlayAudio || sound.Data == null)
            return;

        UpdateIfRequired(sound, out int id);
        sound.State = SoundState.Paused;
    }

    public override void Play(Sound sound, float volume = 1)
    {
        if (!canPlayAudio || sound.Data == null)
            return;

        sound.Volume = volume;
        sound.ForceUpdate();
        EnforceCorrectTrack(sound);
        UpdateIfRequired(sound, out int s);
        sound.State = SoundState.Playing;
        if (!sound.Looping && sound.Data is FixedAudioData)
            AL.SourcePlay(s);
    }

    public override void Play(Sound sound, Vector3 worldPosition, float volume = 1)
    {
        if (!canPlayAudio || sound.Data == null)
            return;

        worldPosition *= SpatialMultiplier;

        sound.Volume = volume;
        sound.ForceUpdate();
        EnforceCorrectTrack(sound);
        UpdateIfRequired(sound, out int s);
        if (sound.SpatialParams.HasValue)
            AL.Source(s, ALSource3f.Position, worldPosition.X, worldPosition.Y, worldPosition.Z);
        else
            Logger.Warn("Attempt to play a non-spatial sound in space!");
        sound.State = SoundState.Playing;
        if (!sound.Looping && sound.Data is FixedAudioData)
            AL.SourcePlay(s);
    }

    private int CreateTempSource(Sound sound, float volume, Vector3 worldPosition, float pitch,
        AudioTrack? track = null)
    {
        worldPosition *= SpatialMultiplier;

        var source = SourceCache.CreateSourceFor(sound);

        AL.Source(source, ALSourceb.Looping, false);
        AL.Source(source, ALSourcef.Gain, (sound.Track?.Muted ?? false) ? 0 : (volume * (sound.Track?.Volume ?? 1)));
        AL.Source(source, ALSourcef.Pitch, pitch * (sound.Track?.Pitch ?? 1));

        if (sound.SpatialParams.HasValue)
            AL.Source(source, ALSource3f.Position, worldPosition.X, worldPosition.Y, worldPosition.Z);
        AL.SourcePlay(source);

        ApplySpatialParams(source, sound);

        temporarySources.RequestObject(new TemporarySourceArgs(
            source,
            sound,
            (float)sound.Data.Duration.TotalSeconds,
            volume,
            track));
        return source;
    }

    public override void PlayOnce(Sound sound, float volume = 1, float pitch = 1, AudioTrack? track = null)
    {
        if (sound.Data is not FixedAudioData)
            throw new Exception("Only fixed buffer audio sources can be overlapped using PlayOnce");

        if (!canPlayAudio || sound.Data == null || (track?.Muted ?? false))
            return;
        UpdateIfRequired(sound, out _);

        try
        {
            CreateTempSource(sound, volume, default, pitch, track ?? sound.Track);
        }
        catch (Exception e)
        {
            Logger.Warn(e);
        }
    }

    public override void PlayOnce(Sound sound, Vector3 worldPosition, float volume = 1, float pitch = 1,
        AudioTrack? track = null)
    {
        if (sound.Data is not FixedAudioData)
            throw new Exception("Only fixed buffer audio sources can be overlapped using PlayOnce");

        if (!canPlayAudio || sound.Data == null || (track?.Muted ?? false))
            return;

        worldPosition *= SpatialMultiplier;

        UpdateIfRequired(sound, out _);
        if (!sound.SpatialParams.HasValue)
            Logger.Warn("Attempt to play a non-spatial sound in space!");

        try
        {
            CreateTempSource(sound, volume, worldPosition, pitch, track ?? sound.Track);
        }
        catch (Exception e)
        {
            Logger.Warn(e);
        }
    }

    public override void Stop(Sound sound)
    {
        if (!canPlayAudio || sound.Data == null)
            return;

        sound.State = SoundState.Stopped;
        UpdateIfRequired(sound, out int s);
        if (sound.Data is not StreamAudioData)
            AL.SourceRewind(s);
    }

    public override void StopAll()
    {
        if (!canPlayAudio)
            return;

        foreach (var sound in AudioObjects.Sources.GetAllUnloaded())
            Stop(sound);

        foreach (var item in temporarySources.GetAllInUse())
        {
            AL.SourceStop(item.Source);
            item.CurrentLifetime = float.MaxValue;
        }
    }

    public override void Release()
    {
        if (!canPlayAudio)
            return;

        canPlayAudio = false;

        AudioObjects.AudioStreamers.UnloadAll();

        foreach (var item in AudioObjects.Sources.GetAllLoaded())
            AL.SourceStop(item);

        AudioObjects.FixedBuffers.UnloadAll();
        AudioObjects.Sources.UnloadAll();

        if (device != ALDevice.Null)
            ALC.CloseDevice(device);

        if (context != ALContext.Null)
        {
            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(context);
        }
    }

    public override void Process(float dt)
    {
        if (!canPlayAudio)
            return;

        foreach (var item in AudioObjects.Sources.GetAll())
        {
            var sound = item.Item1;
            var source = item.Item2;

            if (sound.Data is StreamAudioData)
                continue;

            var sourceState = source.GetALState();
            if (sourceState == ALSourceState.Initial)
                continue;

            switch (sound.State)
            {
                case SoundState.Idle:
                    break;
                case SoundState.Playing:
                    if (sourceState != ALSourceState.Playing && sound.Looping)
                        AL.SourcePlay(source);
                    else if (sourceState == ALSourceState.Stopped)
                        sound.State = SoundState.Stopped;
                    break;
                case SoundState.Paused:
                    if (sourceState == ALSourceState.Playing)
                        AL.SourcePause(source);
                    sound.State = SoundState.Paused;
                    break;
                case SoundState.Stopped:
                    if (sourceState is ALSourceState.Playing or ALSourceState.Paused)
                        AL.SourceStop(source);
                    break;
            }
        }

        //foreach (var streamer in AudioObjects.OggStreamers.GetAllLoaded())
        //    streamer.Update();

        int i = 0;
        foreach (var v in temporarySources.GetAllInUse())
            temporarySourceBuffer[i++] = v;

        for (int j = 0; j < i; j++)
        {
            var v = temporarySourceBuffer[j];
            var pitch = AL.GetSource(v.Source, ALSourcef.Pitch);
            var transformedDuration = v.Duration / pitch;
            if (v.CurrentLifetime > transformedDuration)
            {
                var src = v.Source;
                var buffer = AL.GetSource(src, ALGetSourcei.Buffer);
                if (AudioObjects.SourceByBuffer.TryGetValue(buffer, out var sourceList))
                    sourceList.Remove(src);
                AL.SourceStop(src);
                AL.Source(src, ALSourcei.Buffer, 0);
                AL.DeleteSource(src);
                if (AL.IsSource(src))
                    throw new Exception("wtf");
                temporarySources.ReturnToPool(v);
            }
            else
                v.CurrentLifetime += dt;
        }

        Array.Clear(temporarySourceBuffer);
    }

    public override bool IsPlaying(Sound sound)
    {
        return sound.State == SoundState.Playing ||
               AudioObjects.Sources.Load(sound).GetALState() == ALSourceState.Playing;
    }

    public override void DisposeOf(AudioData audioData)
    {
        if (audioData != null)
        {
            audioData.DisposeLocalCopy();

            if (audioData is FixedAudioData fixedAudioData)
                AudioObjects.FixedBuffers.Unload(fixedAudioData);

            Resources.Unload(audioData);
            if (audioData is IDisposable d)
                d.Dispose();
        }
    }

    public override void DisposeOf(Sound sound)
    {
        if (sound != null)
            AudioObjects.Sources.Unload(sound);
    }

    public override void SetAudioDevice(string device)
    {
        Release();
        Initialise(device);
    }

    public override string GetCurrentAudioDevice()
    {
        if (device == ALDevice.Null)
            return null;

        return ALC.GetString(device, AlcGetString.AllDevicesSpecifier);
    }

    public override IEnumerable<string> EnumerateAvailableAudioDevices()
    {
        if (!canEnumerateDevices)
            yield break;

        foreach (var deviceName in ALC.GetString(AlcGetStringList.AllDevicesSpecifier))
            yield return deviceName;
    }

    public void Resume(Sound sound)
    {
        if (AudioObjects.Sources.Load(sound).GetALState() == ALSourceState.Paused)
            Play(sound);
    }

    public override void PauseAll()
    {
        foreach (var item in AudioObjects.Sources.GetAllUnloaded())
            Pause(item);
    }

    public override void PauseAll(AudioTrack track)
    {
        if (tracks.TryGetValue(track, out var set))
            foreach (var item in set)
                Pause(item);
    }

    public override void ResumeAll()
    {
        foreach (var item in AudioObjects.Sources.GetAllUnloaded())
            Resume(item);
    }

    public override void ResumeAll(AudioTrack track)
    {
        if (tracks.TryGetValue(track, out var set))
            foreach (var item in set)
                Resume(item);
    }

    public override void StopAll(AudioTrack track)
    {
        if (tracks.TryGetValue(track, out var set))
            foreach (var item in set)
                Stop(item);
    }

    private void EnforceCorrectTrack(Sound s)
    {
        //if the sound has been countered
        if (trackBySound.TryGetValue(s, out var cTrack))
        {
            // but the track it claims to be associated with does not match what we have stored
            if (s.Track != cTrack)
            {
                //synchronise
                trackBySound[s] = s.Track;

                if (s.Track != null)
                    addOrCreateTrack();

                //remove from old track
                if (cTrack != null && tracks.TryGetValue(cTrack, out var oldSet))
                    oldSet.Remove(s);
            }
        }
        else //its never been encountered...
        {
            trackBySound.Add(s, s.Track);
            if (s.Track != null)
                addOrCreateTrack();
        }

        void addOrCreateTrack()
        {
            //add to tracks list
            if (tracks.TryGetValue(s.Track, out var set))
                set.Add(s);
            else
            {
                set = new HashSet<Sound>() { s };
                tracks.Add(s.Track, set);
            }
        }
    }

    public override void UpdateTracks()
    {
        foreach (var track in tracks)
        {
            if (track.Key.RequiresUpdate)
            {
                foreach (var sound in track.Value)
                    UpdateIfRequired(sound, out _);
                track.Key.RequiresUpdate = false;
            }
        }

        foreach (var item in AudioObjects.Sources.GetAllUnloaded())
            UpdateIfRequired(item, out _);
    }

    public override void SetTime(Sound sound, float seconds)
    {
        UpdateIfRequired(sound, out var source);

        switch (sound.Data)
        {
            case StreamAudioData:
                var streamer = AudioObjects.AudioStreamers.Load(new AudioStreamerHandle(source, sound));
                streamer.CurrentTime = TimeSpan.FromSeconds(seconds);
                break;
            default:
                AL.Source(source, ALSourcef.SecOffset, seconds);

                break;
        }
    }

    public override float GetTime(Sound sound)
    {
        UpdateIfRequired(sound, out var source);
        switch (sound.Data)
        {
            case StreamAudioData:
                var streamer = AudioObjects.AudioStreamers.Load(new AudioStreamerHandle(source, sound));
                return (float)streamer.CurrentTime.TotalSeconds;
            default:
                AL.GetSource(source, ALSourcef.SecOffset, out var offset);

                return offset;
        }
    }

    public override void SetPosition(Sound sound, Vector3 worldPosition)
    {
        worldPosition *= SpatialMultiplier;

        UpdateIfRequired(sound, out var source);
        if (sound.SpatialParams.HasValue)
        {
            AL.Source(source, ALSource3f.Position, worldPosition.X, worldPosition.Y, worldPosition.Z);
        }
        else
            Logger.Error("Attempt to set position for non-spatial sound");
    }

    /// <summary>
    /// Populates the array with the most recently played samples ranging from -1 to 1
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="arr"></param>
    /// <returns></returns>
    public override int GetCurrentSamples(Sound sound, Span<float> arr)
    {
        UpdateIfRequired(sound, out var source);

        switch (sound.Data)
        {
            case StreamAudioData:
                {
                    var streamer = AudioObjects.AudioStreamers.Load(new AudioStreamerHandle(source, sound));
                    int i = 0;
                    foreach (var item in streamer.TakeLastPlayed(arr.Length))
                        arr[i++] = item;
                    return i;
                }
            case FixedAudioData fixedData:
                {
                    const int amount = 1024;
                    float progress = GetTime(sound) / (float)sound.Data.Duration.TotalSeconds;
                    int total = fixedData.Data.Length - amount;
                    int cursor = Utilities.Clamp((int)(total * progress), 0, fixedData.Data.Length);
                    int maxReturnSize = fixedData.Data.Length - cursor;
                    var section = fixedData.Data.AsSpan(cursor, Math.Min(arr.Length, Math.Min(maxReturnSize, amount)));
                    for (int i = 0; i < section.Length; i++)
                        arr[i] = Utilities.MapRange(0, byte.MaxValue, -1, 1, arr[i]);
                    return section.Length;
                }
            default:
                return 0;
        }
    }
}

internal static class ALUtils
{
    public static void CheckError([CallerMemberName] in string id = default)
    {
        while (true)
        {
            var e = AL.GetError();
            if (e == ALError.NoError)
                break;
            Console.Error.WriteLine($"OpenAL error at [{id ?? "unknown"}]: " + AL.GetErrorString(e));
        }
    }
}