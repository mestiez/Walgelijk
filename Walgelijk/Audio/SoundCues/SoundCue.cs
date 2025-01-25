using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.Audio.SoundCues;

public interface ISoundCue
{
    SoundState State { get; }
    void Play(SoundCueManager manager);
}

public sealed class SoundCueManager(Game game)
{
    public AudioRenderer Renderer => game.AudioRenderer;
}

public abstract class SoundEffectCue : ISoundCue
{
    public FloatRange Pitch = 1;
    public FloatRange Volume = 1;
    public FloatRange Delay = 0;
    public FloatRange Repeat = 0;
    public bool Overlap = true;

    public abstract SoundState State { get; }
    public abstract void Play(SoundCueManager manager);
}

public class SoundCue : SoundEffectCue
{
    public override SoundState State { get; private set; }

    public override void Play(SoundCueManager manager)
    {
        throw new NotImplementedException();
    }
}

public class GroupCue : ISoundCue
{
    public ISoundCue[] Cues { get; }

    public SoundState State { get; private set; }

    public GroupCue(params ISoundCue[] cues)
    {
        Cues = cues;
    }

    public void Play(SoundCueManager manager)
    {
        foreach (var c in Cues)
            c.Play(manager);
    }
}