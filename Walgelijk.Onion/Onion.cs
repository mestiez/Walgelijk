using Walgelijk.Onion.Layout;

namespace Walgelijk.Onion;

public static class Onion
{
    public static readonly Layout.Layout Layout = new();
    public static readonly ControlTree Tree = new();
    public static readonly Navigator Navigator = new();
    public static readonly Input Input = new();
    public static readonly Configuration Configuration = new();
    public static readonly StyleCascade Style = new();

    /*TODO 
     * ClearEverything();
     * Windows!! draggables
     * style
     *      style moet textures meer supporten, niet alleen kleuren 
     *      misschien zelfs iets anders dan quads
     *      uber shader voor alle controls
     * Sounds :)
     * Stack<Style> en dan bouw je voor elke control een final style misschien?
     * heel veel basic functies hier (label, button. etc.)
     * Animation system (IAnimation) deel van style? nee toch??? weet ik het 
     * navigation (arrows, tab)
     *      tab: cycle through all controls chronologically
     *      arrows: move from control to control based on position in space
    */
}

public class StyleCascade : Style
{
    public void SetStyle(Style style)
    {

    }
}

public class Style
{
    public Appearance Background;
    public Color Foreground;

    dit
}

public struct Appearance
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
}