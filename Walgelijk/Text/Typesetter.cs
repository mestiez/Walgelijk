using System;

namespace Walgelijk;

/// <summary>
/// Generates text meshes
/// </summary>
public class Typesetter
{
    public Font Font = Font.Default;

    public Typesetter() { }

    public Typesetter(Font font, int fontSize)
    {
        Font = font;
        FontSize = fontSize;
    }

    public int FontSize = 12;
    public Color Color = Colors.White;

    public float TrackingMultiplier = 1;
    public float KerningMultiplier = 1;
    public float LineHeightMultiplier = 1;

    public float WrappingWidth = float.PositiveInfinity;
    public HorizontalTextAlign HorizontalAlign = HorizontalTextAlign.Left;
    public VerticalTextAlign VerticalAlign = VerticalTextAlign.Top;
    public bool Multiline = true;

    public TextMeshResult Generate(in ReadOnlySpan<char> str, Vertex[] vertices, uint[] indices)
    {
        return new TextMeshResult();
    }

    public float GetWidth(in ReadOnlySpan<char> str)
    {
        return 0;
    }

    public float GetHeight(in ReadOnlySpan<char> str)
    {
        return 0;
    }
}
