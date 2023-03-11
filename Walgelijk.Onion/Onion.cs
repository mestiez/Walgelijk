namespace Walgelijk.Onion;

public static class Onion
{
    public static readonly Layout.Layout Layout = new();
    public static readonly ControlTree Tree = new();
    public static readonly Navigator Navigator = new();
    public static readonly Input Input = new();
    public static readonly Configuration Configuration = new();
    public static Theme Theme = new();

    public static readonly Material ControlMaterial = OnionMaterial.CreateNew();

    /*TODO 
     * ClearEverything();
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
