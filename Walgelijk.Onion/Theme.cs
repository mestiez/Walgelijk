using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public struct Theme
{
    public StateDependent<Appearance> Background = new(new Color(0x151122));
    public StateDependent<Appearance> Foreground = new(
        new Color(0x403663),
        new Color(0x403663).Brightness(1.1f),
        new Color(0x403663).Brightness(0.9f),
        new Color(0x403663).Brightness(0.8f));

    public StateDependent<Color> Text = new Color(0xf0f0f0);
    public StateDependent<Color> Accent = new Color(0xcc820a);
    public StateDependent<Color> Image = Colors.White;
    public Color Highlight = new Color(0xffffff);

    public Font Font = Font.Default;
    public StateDependent<int> FontSize = 12;

    public int Padding = 5;
    public float Rounding = 2;

    public StateDependent<int> OutlineWidth = new StateDependent<int>(1, 2, 1, 2);
    public StateDependent<Color> OutlineColour = Colors.Black;

    public StateDependent<float> WindowTitleBarHeight = 24;

    public Color FocusBoxColour = new Color(0xFFA500);
    public float FocusBoxSize = 5;
    public float FocusBoxWidth = 4;

    public bool ShowScrollbars = true;
    public float ScrollbarWidth = 16;
    public StateDependent<Color> ScrollbarTracker = new(new Color(0xcc820a), new Color(0xcc820a).Brightness(1.2f), new Color(0xcc820a).Brightness(0.9f));
    public StateDependent<Color> ScrollbarBackground = new Color(0x151122).Brightness(2f);

    private bool wasScaled = false;

    public Theme()
    {
    }

    public readonly Theme WithBackgroundColor(StateDependent<Color> color)
    {
        var copy = this;
        copy.Background.SetColor(color);
        return copy;
    }

    public readonly Theme WithForegroundColor(StateDependent<Color> color)
    {
        var copy = this;
        copy.Foreground.SetColor(color);
        return copy;
    }

    public readonly Theme WithBackgroundTexture(IReadableTexture tex, ImageMode mode)
    {
        var copy = this;
        copy.Background.SetTexture(tex, mode);
        return copy;
    }

    public readonly Theme WithForegroundTexture(IReadableTexture tex, ImageMode mode)
    {
        var copy = this;
        copy.Foreground.SetTexture(tex, mode);
        return copy;
    }

    public void ApplyScaling(float scale)
    {
        if (wasScaled)
            return;
        wasScaled = true;

        ScrollbarWidth *= scale;
        FocusBoxSize *= scale;
        FocusBoxWidth *= scale;
        Padding = (int)(Padding * scale);
        Rounding *= scale;

        WindowTitleBarHeight.Scale(scale);
        OutlineWidth.Scale(scale);
        FontSize.Scale(scale);
    }
}
