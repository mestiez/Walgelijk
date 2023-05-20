namespace Walgelijk.Onion;

public struct Theme
{
    public StateDependent<Appearance> Background = new(new Color("#151122"));
    public StateDependent<Appearance> Foreground = new(
        new Color("#403663"), 
        new Color("#403663").Brightness(1.1f),
        new Color("#403663").Brightness(0.9f), 
        new Color("#403663").Brightness(0.8f));

    public StateDependent<Color> Text = new Color("#f0f0f0");
    public StateDependent<Color> Accent = new Color("#cc820a");
    public Color Highlight = new Color("#ffffff");

    public Font Font = Font.Default;
    public StateDependent<int> FontSize = 12;

    public int Padding = 5;
    public float Rounding = 2;

    public StateDependent<int> OutlineWidth = new StateDependent<int>(1, 2, 1, 2);
    public StateDependent<Color> OutlineColour = Colors.Black;

    public StateDependent<float> WindowTitleBarHeight = 24;

    public Color FocusBoxColour = new Color("#FFA500");
    public float FocusBoxSize = 5;
    public float FocusBoxWidth = 4;

    public bool ShowScrollbars = true;
    public float ScrollbarWidth = 24;

    public Theme()
    {
    }
}
