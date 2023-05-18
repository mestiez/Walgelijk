namespace Walgelijk.Onion;

public struct Theme
{
    public StateDependent<Appearance> Background = (Appearance)new Color("#BCBCBC");
    public StateDependent<Appearance> Foreground = new(
        new Color("#D7D4D4"), 
        new Color("#D7D4D4") * 1.1f, 
        new Color("#D7D4D4") * 0.9f, 
        new Color("#D7D4D4") * 0.8f);

    public StateDependent<Color> Text = new Color("#3B3B3B");
    public StateDependent<Color> Accent = new Color("#D8B038");
    public Color Highlight = new Color("#ffffff");

    public Font Font = Font.Default;
    public StateDependent<int> FontSize = 12;

    public int Padding = 5;
    public float Rounding = 1;

    public StateDependent<int> OutlineWidth = new StateDependent<int>(1, 2, 1, 2);
    public StateDependent<Color> OutlineColour = new Color("#838383");

    public StateDependent<float> WindowTitleBarHeight = 24;

    public Color FocusBoxColour = new Color("#ffc61c");
    public float FocusBoxSize = 5;
    public float FocusBoxWidth = 4;

    public Theme()
    {
    }
}
