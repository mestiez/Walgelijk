namespace Walgelijk.Onion.Animations;

public class AnimationQueue
{
    public List<IAnimation> Default = new() { new FadeAnimation() };
    public float DefaultDurationSeconds = 0.1f;

    public readonly Queue<IAnimation> Queue = new();
    internal float TargetDurationSeconds = 0;
    internal bool ForceNoAnimation = false;

    public void SetDuration(float seconds)
    {
        TargetDurationSeconds = seconds;
    }

    public void Add(in IAnimation anim)
    {
        Queue.Enqueue(anim);
    }

    public void Clear()
    {
        Queue.Clear();
    }

    public void DoNotAnimate() => ForceNoAnimation = true;

    public IEasing Easing = new IEasing.Cubic();

    public void Process(ControlInstance inst)
    {
        if (!ForceNoAnimation)
        {
            if (Queue.Count == 0)
            {
                if (inst.Animations.All.Length != Default.Count)
                    inst.Animations.All = new IAnimation[Default.Count];
                for (int i = 0; i < Default.Count; i++)
                    inst.Animations.All[i] = Default[i];
            }
            else
            {
                int i = 0;
                if (inst.Animations.All.Length != Queue.Count)
                    inst.Animations.All = new IAnimation[Queue.Count];
                while (Queue.TryDequeue(out var next))
                    inst.Animations.All[i++] = next;
            }
        }
        else
        {
            Queue.Clear();
            inst.Animations.All = Array.Empty<IAnimation>();
        }

        inst.AllowedDeadTime = TargetDurationSeconds < 0 ? DefaultDurationSeconds : TargetDurationSeconds;
        TargetDurationSeconds = -1;
        ForceNoAnimation = false;
    }
}
