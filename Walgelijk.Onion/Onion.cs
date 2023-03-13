using System.Reflection;
using Walgelijk.Onion.Animations;

namespace Walgelijk.Onion;

public static class Onion
{
    public static readonly Layout.Layout Layout = new();
    public static readonly ControlTree Tree = new();
    public static readonly Navigator Navigator = new();
    public static readonly Input Input = new();
    public static readonly Configuration Configuration = new();
    public static readonly Animations Animation = new();
    public static Theme Theme = new();

    public static readonly Material ControlMaterial = OnionMaterial.CreateNew();

    static Onion()
    {
        CommandProcessor.RegisterAssembly(Assembly.GetAssembly(typeof(Onion)) ?? throw new Exception("I do not exist."));
    }

    [Command(Alias = "OnionClear", HelpString = "Clears the Onion UI cache, effectively resetting the UI scene")]
    public static void ClearCache()
    {
        Layout.Reset();
        Tree.Clear();
        Navigator.Clear();
    }

    /*TODO 
     * FIX NESTED ORDERS:
     *      andere base? base 1000
     *      hoe tf sla je dit op in int32
     * Windows!! draggables
     * scrollbars etc. (pseudo controls)
     * style
     *      style moet textures meer supporten, niet alleen kleuren 
     *      misschien zelfs iets anders dan quads
     *      uber shader voor alle controls
     * Sounds :)
     * Stack<Style> en dan bouw je voor elke control een final style misschien?
     * heel veel basic functies hier (label, button. etc.)
     * Animation system (IAnimation) deel van style? nee toch??? weet ik het 
    */

    public class Animations
    {
        public readonly IList<IAnimation> Default = new IAnimation[] { new FadeAnimation() };
        public float DefaultDurationSeconds = 0.3f;

        public readonly Queue<IAnimation> AnimationQueue = new();
        internal float TargetDurationSeconds = 0;
        internal bool ForceNoAnimation = false;

        public void SetDuration(float seconds)
        {
            TargetDurationSeconds = seconds;
        }

        public void Add(in IAnimation anim)
        {
            AnimationQueue.Enqueue(anim);
        }

        public void DoNotAnimate() => ForceNoAnimation = true;

        public void Process(ControlInstance inst)
        {
            if (!ForceNoAnimation)
            {
                if (AnimationQueue.Count == 0)
                {
                    if (inst.Animations.All.Length != Default.Count)
                        inst.Animations.All = new IAnimation[Default.Count];
                    for (int i = 0; i < Default.Count; i++)
                        inst.Animations.All[i] = Default[i];
                }
                else
                {
                    int i = 0;
                    if (inst.Animations.All.Length != AnimationQueue.Count)
                        inst.Animations.All = new IAnimation[AnimationQueue.Count];
                    while (AnimationQueue.TryDequeue(out var next))
                        inst.Animations.All[i++] = next;
                }
            }
            else
            {
                AnimationQueue.Clear();
                inst.Animations.All = Array.Empty<IAnimation>();
            }

            inst.AllowedDeadTime = TargetDurationSeconds < 0 ? DefaultDurationSeconds : TargetDurationSeconds;
            TargetDurationSeconds = -1;
            ForceNoAnimation = false;
        }
    }
}

public class Theme
{
    public Appearance Background = new Color("#022525");
    public Appearance Foreground = new Color("#055555");
    public Color Text = new Color("#fcffff");
    public Color Accent = new Color("#de3a67");
    public Color Highlight = new Color("#ffffff");

    public Font Font = Walgelijk.Font.Default;
    public int FontSize = 12;

    public float Padding = 5;
    public float Rounding = 1;

    public Color FocusBoxColour = new Color("#3adeda");
    public float FocusBoxSize = 5;
    public float FocusBoxWidth = 4;
}

public class ThemeProperty<T> where T : notnull
{
    public readonly T Default;

    public ThemeProperty(in T @default)
    {
        Default = @default;
    }

    private readonly Stack<T> stack = new();

    public void Push(T val) => stack.Push(val);

    public T Pop() => stack.Pop();

    public T Get()
    {
        if (stack.TryPeek(out var val))
            return val;
        return Default;
    }

    public static implicit operator T(ThemeProperty<T> theme) => theme.Get();
}
