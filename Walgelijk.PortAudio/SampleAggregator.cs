using i3arnon.ConcurrentCollections;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.PortAudio.Voices;

namespace Walgelijk.PortAudio;

internal class SampleAggregator : IDisposable
{
    public readonly int MaxVoices, ProcessInterval;

    public float Volume { get; set; } = 1;
    public bool Muted { get; set; }
    public Vector3 ListenerPosition { get; set; }
    public (Vector3 Forward, Vector3 Up) ListenerOrientation { get; set; }
    public AudioDistanceModel DistanceModel { get; set; }

    private IVoice[] sharedVoiceCache = [];

    public IEnumerable<IVoice> GetAll()
    {
        foreach (var item in sharedVoiceCache)
            yield return item;

        foreach (var item in oneShotVoices)
            yield return item;
    }

    public IEnumerable<IVoice> GetAll(AudioTrack track)
    {
        foreach (var item in sharedVoiceCache)
            if (item.Track == track)
                yield return item;

        foreach (var item in oneShotVoices)
            if (item.Track == track)
                yield return item;
    }

    private readonly ConcurrentDictionary<Sound, IVoice> sharedVoices = [];
    private readonly ConcurrentHashSet<IVoice> oneShotVoices = [];
    private readonly Thread processThread;

    //private readonly SemaphoreSlim sharedvoiceSetLock = new(1);

    private bool isDisposed = false;

    public SampleAggregator(int maxVoices, int processInterval)
    {
        MaxVoices = maxVoices; 
        ProcessInterval = processInterval;

        processThread = new(Process)
        {
            IsBackground = true
        };
        processThread.Start();
    }

    private void Process(object? _)
    {
        var removeStack = new Stack<IVoice>();

        while (!isDisposed)
        {
            Thread.Sleep(ProcessInterval);

            foreach (var s in oneShotVoices)
                if (s.IsFinished)
                    removeStack.Push(s);

            while (removeStack.TryPop(out var p))
                oneShotVoices.TryRemove(p);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetShared(Sound sound, out IVoice voice)
    {
        if (sharedVoices.TryGetValue(sound, out voice))
            return;

        voice = sound.Data is StreamAudioData ? new SharedStreamVoice(sound) : new SharedFixedVoice(sound);
        sharedVoices.AddOrSet(sound, voice);
        sharedVoiceCache = [.. sharedVoices.Values];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetOneShot(Sound sound, out IVoice voice)
    {
        voice = sound.Data is StreamAudioData ? new OneShotStreamVoice(sound) : new OneShotFixedVoice(sound);
        oneShotVoices.Add(voice);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Sound sound) => sharedVoices.ContainsKey(sound);

    public void Dispose()
    {
        sharedVoiceCache = null!;
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

        foreach (var voice in sharedVoiceCache)
            voice.GetSamples(buffer);

        foreach (var voice in oneShotVoices)
            voice.GetSamples(buffer);

        for (int i = 0; i < buffer.Length; i++)
            buffer[i] *= Volume;
    }
}
