using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Walgelijk.AssetManager;

namespace Walgelijk.Audio.SoundCues;

public class AssetCue : SoundEffectCue
{
    public override SoundState State => state;
    public GlobalAssetId[] Assets = [];

    public AssetCue(GlobalAssetId[] sounds)
    {
        Assets = sounds;
    }

    private SoundState state;

    public override void Play(SoundCueManager manager)
    {
        if (!Overlap)
        {
            var asset = Utilities.PickRandom(Assets);
            
        }
    }
}
