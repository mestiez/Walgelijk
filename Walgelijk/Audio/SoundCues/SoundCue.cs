using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.Audio.SoundCues;

public interface ISoundCue
{
    SoundState State { get; }
    void Play();
}

public sealed class SoundCueManager
{

}

public class SingleCue : ISoundCue
{
    public SoundState State { get; private set; }

    public SingleCue()
    {
        
    }

    public void Play()
    {
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

    public void Play()
    {
        foreach (var c in Cues)
            c.Play();
    }
}