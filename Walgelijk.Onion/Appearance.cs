namespace Walgelijk.Onion;

public struct Appearance : IEquatable<Appearance>
{
    public Color Color;
    public IReadableTexture Texture;

    public Appearance(Color color, IReadableTexture? texture = null)
    {
        Color = color;
        Texture = texture ?? Walgelijk.Texture.White;
    }

    public Appearance(IReadableTexture texture)
    {
        Color = Colors.White;
        Texture = texture;
    }

    public static implicit operator Appearance(Color color) => new(color);
    public static implicit operator Appearance(Texture texture) => new(texture);
    public static implicit operator Appearance(RenderTexture texture) => new(texture);

    public static bool operator ==(Appearance left, Appearance right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Appearance left, Appearance right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        return obj is Appearance appearance && Equals(appearance);
    }

    public bool Equals(Appearance other)
    {
        return Color.Equals(other.Color) &&
               EqualityComparer<IReadableTexture>.Default.Equals(Texture, other.Texture);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Color, Texture);
    }
}