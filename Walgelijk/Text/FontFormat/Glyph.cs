namespace Walgelijk;

public readonly struct Glyph
{
    public readonly char Character;
    public readonly float Advance;
    public readonly Rect GeometryRect;
    public readonly Rect TextureRect;

    public Glyph(char character, float advance, Rect geometryRect, Rect textureRect)
    {
        Character = character;
        Advance = advance;
        GeometryRect = geometryRect;
        TextureRect = textureRect;
    }
}
