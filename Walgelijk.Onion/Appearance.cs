using Walgelijk.SimpleDrawing;

namespace Walgelijk.Onion;

public struct Appearance : IEquatable<Appearance>
{
    public Color Color;
    public IReadableTexture Texture;
    public ImageMode ImageMode;

    public Appearance(Color color, IReadableTexture? texture = null, ImageMode imageMode = default)
    {
        Color = color;
        Texture = texture ?? Walgelijk.Texture.White;
        ImageMode = imageMode;
    }

    public Appearance(IReadableTexture texture, ImageMode imageMode = default)
    {
        Color = Colors.White;
        Texture = texture;
        ImageMode = imageMode;
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
               ImageMode.Equals(other.ImageMode) &&
               EqualityComparer<IReadableTexture>.Default.Equals(Texture, other.Texture);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Color, Texture, ImageMode);
    }
}