using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.OpenTK;

internal readonly struct TemporarySourceArgs
{
    public readonly int Source;
    public readonly Sound Sound;
    public readonly float Duration;
    public readonly float Volume;
    public readonly AudioTrack? Track;

    public TemporarySourceArgs(int source, Sound sound, float duration, float volume, AudioTrack track)
    {
        Source = source;
        Sound = sound;
        Duration = duration;
        Volume = volume;
        Track = track;
    }
}

internal class TemporarySourcePool : Pool<TemporarySource?, TemporarySourceArgs>
{
    public TemporarySourcePool(int maxCapacity) : base(maxCapacity)
    {
    }

    protected override TemporarySource? CreateFresh() => new();

    protected override TemporarySource? GetOverCapacityFallback() => null;

    protected override void ResetObjectForNextUse(TemporarySource? obj, TemporarySourceArgs initialiser)
    {
        obj.Sound = initialiser.Sound;
        obj.Source = initialiser.Source;
        obj.Duration = initialiser.Duration;
        obj.Volume = initialiser.Volume;
        obj.CurrentLifetime = 0;
    }
}

public class OpenALAudioRenderer : AudioRenderer
{
    internal const int StreamingBufferSize = 1024;
    public readonly int MaxTempSourceCount = 256;

    private ALDevice device;
    private ALContext context;
    private bool canPlayAudio = false;
    private bool canEnumerateDevices = false;
    private readonly TemporarySourcePool temporarySources;

    private readonly Dictionary<AudioTrack, HashSet<Sound>> tracks = new();
    private readonly Dictionary<Sound, AudioTrack?> trackBySound = new();
    private readonly TemporarySource[] temporySourceBuffer;

    public override float Volume
    {
        get
        {
            AL.GetListener(ALListenerf.Gain, out var gain);
            return gain;
        }

        set => AL.Listener(ALListenerf.Gain, value);
    }
    public override bool Muted { get => Volume <= float.Epsilon; set => Volume = 0; }
    public override Vector3 ListenerPosition
    {
        get
        {
            AL.GetListener(ALListener3f.Position, out float x, out float depth, out float z);
            return new Vector3(x, z, depth);
        }

        set => AL.Listener(ALListener3f.Position, value.X, value.Z, value.Y);
    }

    public OpenALAudioRenderer(int maxTempSourceCount = 256)
    {
        MaxTempSourceCount = maxTempSourceCount;
        temporarySources = new(MaxTempSourceCount);
        temporySourceBuffer = new TemporarySource[MaxTempSourceCount];
        Resources.RegisterType(typeof(AudioData), d => LoadSound(d));
        canEnumerateDevices = AL.IsExtensionPresent("ALC_ENUMERATION_EXT");
        Initialise();
    }

    private void Initialise(string? deviceName = null)
    {
        canPlayAudio = false;

        device = ALC.OpenDevice(deviceName);

        if (device == ALDevice.Null)
            Logger.Warn(deviceName == null ? "No audio device could be found" : "The requested audio device could not be found", nameof(OpenALAudioRenderer));
        context = ALC.CreateContext(device, new ALContextAttributes());
        if (context == ALContext.Null)
            Logger.Warn("No audio context could be created", nameof(OpenALAudioRenderer));

        bool couldSetContext = ALC.MakeContextCurrent(context);

        canPlayAudio = device != ALDevice.Null && context != ALContext.Null && couldSetContext;

        if (!couldSetContext)
            Logger.Warn("The audio context could not be set", nameof(OpenALAudioRenderer));

        if (!canPlayAudio)
            Logger.Error("Failed to initialise the audio renderer", nameof(OpenALAudioRenderer));
    }

    private void UpdateIfRequired(Sound sound, out int source)
    {
        source = AudioObjects.Sources.Load(sound);

        if (!sound.RequiresUpdate && !(sound.Track?.RequiresUpdate ?? false))
            return;

        sound.RequiresUpdate = false;

        AL.Source(source, ALSourceb.SourceRelative, !sound.Spatial);
        AL.Source(source, ALSourceb.Looping, sound.Looping);
        AL.Source(source, ALSourcef.RolloffFactor, sound.RolloffFactor);
        AL.Source(source, ALSourcef.Pitch, sound.Pitch * (sound.Track?.Pitch ?? 1));
        AL.Source(source, ALSourcef.Gain, (sound.Track?.Muted ?? false) ? 0 : (sound.Volume * (sound.Track?.Volume ?? 1)));
    }

    public override AudioData LoadSound(string path, bool streaming = false)
    {
        var ext = path.AsSpan()[path.LastIndexOf('.')..];
        AudioFileData data;

        if (streaming)
            throw new NotImplementedException("This audio renderer can not stream yet.");

        try
        {
            if (Utilities.TextEqualsCaseInsensitive(ext, ".wav"))
                data = WaveFileReader.Read(path);
            else if (Utilities.TextEqualsCaseInsensitive(ext, ".ogg"))
                data = VorbisFileReader.Read(path);
            else
                throw new Exception($"This is not a supported audio file. Only Microsoft WAV and Ogg Vorbis can be decoded.");
        }
        catch (Exception e)
        {
            throw new AggregateException($"Failed to load audio file: {path}", e);
        }
        var audio = new AudioData(data.Data, data.SampleRate, data.NumChannels, data.SampleCount);
        return audio;
    }

    public override void Pause(Sound sound)
    {
        if (!canPlayAudio || sound.Data == null)
            return;

        UpdateIfRequired(sound, out int id);
        AL.SourcePause(id);
    }

    public override void Play(Sound sound, float volume = 1)
    {
        if (!canPlayAudio || sound.Data == null)
            return;

        sound.Volume = volume;
        sound.ForceUpdate();
        EnforceCorrectTrack(sound);
        UpdateIfRequired(sound, out int s);
        AL.SourcePlay(s);
    }

    public override void Play(Sound sound, Vector2 worldPosition, float volume = 1)
    {
        if (!canPlayAudio || sound.Data == null)
            return;

        sound.Volume = volume;
        sound.ForceUpdate();
        EnforceCorrectTrack(sound);
        UpdateIfRequired(sound, out int s);
        if (sound.Spatial)
            AL.Source(s, ALSource3f.Position, worldPosition.X, 0, worldPosition.Y);
        else
            Logger.Warn("Attempt to play a non-spatial sound in space!");
        AL.SourcePlay(s);
    }

    private int CreateTempSource(Sound sound, float volume, Vector2 worldPosition, float pitch, AudioTrack? track = null)
    {
        var source = SourceCache.CreateSourceFor(sound);
        AL.Source(source, ALSourceb.SourceRelative, !sound.Spatial);
        AL.Source(source, ALSourceb.Looping, false);
        AL.Source(source, ALSourcef.Gain, (sound.Track?.Muted ?? false) ? 0 : (volume * (sound.Track?.Volume ?? 1)));
        AL.Source(source, ALSourcef.Pitch, pitch * (sound.Track?.Pitch ?? 1));
        if (sound.Spatial)
            AL.Source(source, ALSource3f.Position, worldPosition.X, 0, worldPosition.Y);
        AL.SourcePlay(source);
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
        if (!canPlayAudio || sound.Data == null || (track?.Muted ?? false))
            return;

        UpdateIfRequired(sound, out _);
        CreateTempSource(sound, volume, default, pitch, track ?? sound.Track);
    }

    public override void PlayOnce(Sound sound, Vector2 worldPosition, float volume = 1, float pitch = 1, AudioTrack? track = null)
    {
        if (!canPlayAudio || sound.Data == null || (track?.Muted ?? false))
            return;

        UpdateIfRequired(sound, out _);
        if (!sound.Spatial)
            Logger.Warn("Attempt to play a non-spatial sound in space!");
        CreateTempSource(sound, volume, worldPosition, pitch, track ?? sound.Track);
    }

    public override void Stop(Sound sound)
    {
        if (!canPlayAudio || sound.Data == null)
            return;

        UpdateIfRequired(sound, out int s);
        AL.SourceStop(s);
    }

    public override void StopAll()
    {
        if (!canPlayAudio)
            return;

        foreach (var sound in AudioObjects.Sources.GetAllLoaded())
            AL.SourceStop(sound);

        foreach (var item in temporarySources.GetAllInUse())
        {
            AL.SourceStop(item.Source);
            item.CurrentLifetime = float.MaxValue;
        }
    }

    public override void Release()
    {
        canPlayAudio = false;

        if (device != ALDevice.Null)
            ALC.CloseDevice(device);

        if (context != ALContext.Null)
        {
            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(context);
        }

        foreach (var item in AudioObjects.Buffers.GetAllUnloaded())
            DisposeOf(item);

        foreach (var item in AudioObjects.Sources.GetAllUnloaded())
            DisposeOf(item);

        AudioObjects.Buffers.UnloadAll();
        AudioObjects.Sources.UnloadAll();
    }

    public override void Process(Game game)
    {
        if (!canPlayAudio)
            return;

        int i = 0;
        foreach (var v in temporarySources.GetAllInUse())
            temporySourceBuffer[i++] = v;

        foreach (var v in temporySourceBuffer.AsSpan()[0..i])
        {
            if (v.CurrentLifetime > v.Duration)
            {
                AL.DeleteSource(v.Source);
                temporarySources.ReturnToPool(v);
            }
            else 
                v.CurrentLifetime += game.Time.DeltaTime;
        }

        Array.Clear(temporySourceBuffer);
    }

    public override bool IsPlaying(Sound sound)
    {
        return AL.GetSourceState(AudioObjects.Sources.Load(sound)) == ALSourceState.Playing;
    }

    public override void DisposeOf(AudioData audioData)
    {
        if (audioData != null)
        {
            audioData.DisposeLocalCopy();
            AudioObjects.Buffers.Unload(audioData);
            Resources.Unload(audioData);
        }
        //TODO dispose of vorbis reader if applicable
        //if (AudioObjects.VorbisReaderCache.Has())
        //AudioObjects.VorbisReaderCache.Unload(audioData);
    }

    public override void DisposeOf(Sound sound)
    {
        if (sound != null)
        {
            AudioObjects.Sources.Unload(sound);
            Resources.Unload(sound);
        }
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
        {
            Logger.Warn("ALC_ENUMERATION_EXT is not present");
            yield break;
        }

        foreach (var deviceName in ALC.GetString(AlcGetStringList.AllDevicesSpecifier))
            yield return deviceName;
    }

    public void Resume(Sound sound)
    {
        if (AL.GetSourceState(AudioObjects.Sources.Load(sound)) == ALSourceState.Paused)
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
                set = new HashSet<Sound>();
                set.Add(s);
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
        AL.Source(source, ALSourcef.SecOffset, seconds);
    }

    public override float GetTime(Sound sound)
    {
        UpdateIfRequired(sound, out var source);
        AL.GetSource(source, ALSourcef.SecOffset, out var offset);
        return offset;
    }
}
