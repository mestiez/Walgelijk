using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Colour with 4 floating point components ranging from 0-1
/// </summary>
public struct Color
{
    /// <summary>
    /// Size of an instance of this struct in bytes
    /// </summary>
    public const int Stride = sizeof(float) * 4;

    /// <summary>
    /// Red component
    /// </summary>
    public float R;
    /// <summary>
    /// Green component
    /// </summary>
    public float G;
    /// <summary>
    /// Blue component
    /// </summary>
    public float B;
    /// <summary>
    /// Alpha component
    /// </summary>
    public float A;

    /// <summary>
    /// Create a colour using floating point values ranging from 0 to 1 in standard dynamic range
    /// </summary>
    public Color(float r, float g, float b, float a = 1)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Create a colour using bytes where 0 is 0.0 and 255 is 1.0
    /// </summary>
    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r / 255f;
        G = g / 255f;
        B = b / 255f;
        A = a / 255f;
    }

    /// <summary>
    /// Create a colour based on a hexadecimal representation, such as "#d42c5e"
    /// </summary>
    public Color(ReadOnlySpan<char> hex)
    {
        if (hex.StartsWith("#"))
            hex = hex[1..];
        else if (hex.StartsWith("0x"))
            hex = hex[2..];

        if (hex.Length < 6)
            throw new ArgumentException(hex.ToString() + " is not a valid hexadecimal representation of a colour");

        R = byte.Parse(hex[0..2], global::System.Globalization.NumberStyles.HexNumber) / 255f;
        G = byte.Parse(hex[2..4], global::System.Globalization.NumberStyles.HexNumber) / 255f;
        B = byte.Parse(hex[4..6], global::System.Globalization.NumberStyles.HexNumber) / 255f;

        if (hex.Length == 8)
            A = byte.Parse(hex[6..8], global::System.Globalization.NumberStyles.HexNumber) / 255f;
        else
            A = 1;
    }

    /// <summary>
    /// Bright vermilion
    /// </summary>
    public static Color Red => new Color("#AE1C28");
    /// <summary>
    /// White
    /// </summary>
    public static Color White => new Color(1f, 1f, 1f);
    /// <summary>
    /// Cobalt blue
    /// </summary>
    public static Color Blue => new Color("#21468B");
    /// <summary>
    /// Orange
    /// </summary>
    public static Color Orange => new Color("#FF4F00");

    /// <summary>
    /// Return a copy of the colour with the given alpha
    /// </summary>
    public Color WithAlpha(float alpha) => new Color(R, G, B, alpha);

    /// <summary>
    /// Returns a tuple where each element corresponds with a component of the colour
    /// </summary>
    public (byte r, byte g, byte b, byte a) ToBytes()
    {
        byte r = (byte)(Utilities.Clamp(R) * 255);
        byte g = (byte)(Utilities.Clamp(G) * 255);
        byte b = (byte)(Utilities.Clamp(B) * 255);
        byte a = (byte)(Utilities.Clamp(A) * 255);

        return (r, g, b, a);
    }

    public override string ToString()
    {
        return $"({R},{G},{B},{A})";
    }

    public override bool Equals(object? obj)
    {
        return obj is Color color &&
               R == color.R &&
               G == color.G &&
               B == color.B &&
               A == color.A;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.R == right.R &&
               left.G == right.G &&
               left.B == right.B &&
               left.A == right.A;
    }

    public static bool operator !=(Color left, Color right)
    {
        return !(left == right);
    }

    public static Color operator *(Color left, Color right)
    {
        return new Color(
            left.R * right.R,
            left.G * right.G,
            left.B * right.B,
            left.A * right.A
            );
    }

    public static Color operator *(float v, Color c)
    {
        return new Color(
            c.R * v,
            c.G * v,
            c.B * v,
            c.A * v
            );
    }

    public static Color operator *(Color c, float v)
    {
        return new Color(
            c.R * v,
            c.G * v,
            c.B * v,
            c.A * v
            );
    }

    public static bool operator >(Color left, Color right)
    {
        return
            left.R > right.R &&
            left.G > right.G &&
            left.B > right.B &&
            left.A > right.A;
    }

    public static bool operator <(Color left, Color right)
    {
        return
            left.R < right.R &&
            left.G < right.G &&
            left.B < right.B &&
            left.A < right.A;
    }
    public static bool operator >=(Color left, Color right)
    {
        return
            left.R >= right.R &&
            left.G >= right.G &&
            left.B >= right.B &&
            left.A >= right.A;
    }

    public static bool operator <=(Color left, Color right)
    {
        return
            left.R <= right.R &&
            left.G <= right.G &&
            left.B <= right.B &&
            left.A <= right.A;
    }

    public static implicit operator Color(Vector4 value)
    {
        return new Color(value.X, value.Y, value.Z, value.W);
    }

    public static implicit operator Vector4(Color value)
    {
        return new Vector4(value.R, value.G, value.B, value.A);
    }

    public static implicit operator Color((float r, float g, float b, float a) value)
    {
        return new Color(value.r, value.g, value.b, value.a);
    }

    public static implicit operator (float r, float g, float b, float a)(Color value)
    {
        return (value.R, value.G, value.B, value.A);
    }

    public readonly Vector3 RRR => new(R, R, R);
    public readonly Vector3 RRG => new(R, R, G);
    public readonly Vector3 RRB => new(R, R, B);
    public readonly Vector3 RRA => new(R, R, A);
    public readonly Vector3 RGR => new(R, G, R);
    public readonly Vector3 RGG => new(R, G, G);
    public readonly Vector3 RGB => new(R, G, B);
    public readonly Vector3 RGA => new(R, G, A);
    public readonly Vector3 RBR => new(R, B, R);
    public readonly Vector3 RBG => new(R, B, G);
    public readonly Vector3 RBB => new(R, B, B);
    public readonly Vector3 RBA => new(R, B, A);
    public readonly Vector3 RAR => new(R, A, R);
    public readonly Vector3 RAG => new(R, A, G);
    public readonly Vector3 RAB => new(R, A, B);
    public readonly Vector3 RAA => new(R, A, A);
    public readonly Vector3 GRR => new(G, R, R);
    public readonly Vector3 GRG => new(G, R, G);
    public readonly Vector3 GRB => new(G, R, B);
    public readonly Vector3 GRA => new(G, R, A);
    public readonly Vector3 GGR => new(G, G, R);
    public readonly Vector3 GGG => new(G, G, G);
    public readonly Vector3 GGB => new(G, G, B);
    public readonly Vector3 GGA => new(G, G, A);
    public readonly Vector3 GBR => new(G, B, R);
    public readonly Vector3 GBG => new(G, B, G);
    public readonly Vector3 GBB => new(G, B, B);
    public readonly Vector3 GBA => new(G, B, A);
    public readonly Vector3 GAR => new(G, A, R);
    public readonly Vector3 GAG => new(G, A, G);
    public readonly Vector3 GAB => new(G, A, B);
    public readonly Vector3 GAA => new(G, A, A);
    public readonly Vector3 BRR => new(B, R, R);
    public readonly Vector3 BRG => new(B, R, G);
    public readonly Vector3 BRB => new(B, R, B);
    public readonly Vector3 BRA => new(B, R, A);
    public readonly Vector3 BGR => new(B, G, R);
    public readonly Vector3 BGG => new(B, G, G);
    public readonly Vector3 BGB => new(B, G, B);
    public readonly Vector3 BGA => new(B, G, A);
    public readonly Vector3 BBR => new(B, B, R);
    public readonly Vector3 BBG => new(B, B, G);
    public readonly Vector3 BBB => new(B, B, B);
    public readonly Vector3 BBA => new(B, B, A);
    public readonly Vector3 BAR => new(B, A, R);
    public readonly Vector3 BAG => new(B, A, G);
    public readonly Vector3 BAB => new(B, A, B);
    public readonly Vector3 BAA => new(B, A, A);
    public readonly Vector3 ARR => new(A, R, R);
    public readonly Vector3 ARG => new(A, R, G);
    public readonly Vector3 ARB => new(A, R, B);
    public readonly Vector3 ARA => new(A, R, A);
    public readonly Vector3 AGR => new(A, G, R);
    public readonly Vector3 AGG => new(A, G, G);
    public readonly Vector3 AGB => new(A, G, B);
    public readonly Vector3 AGA => new(A, G, A);
    public readonly Vector3 ABR => new(A, B, R);
    public readonly Vector3 ABG => new(A, B, G);
    public readonly Vector3 ABB => new(A, B, B);
    public readonly Vector3 ABA => new(A, B, A);
    public readonly Vector3 AAR => new(A, A, R);
    public readonly Vector3 AAG => new(A, A, G);
    public readonly Vector3 AAB => new(A, A, B);
    public readonly Vector3 AAA => new(A, A, A);
    public readonly Vector2 RR => new(R, R);
    public readonly Vector2 RG => new(R, G);
    public readonly Vector2 RB => new(R, B);
    public readonly Vector2 RA => new(R, A);
    public readonly Vector2 GR => new(G, R);
    public readonly Vector2 GG => new(G, G);
    public readonly Vector2 GB => new(G, B);
    public readonly Vector2 GA => new(G, A);
    public readonly Vector2 BR => new(B, R);
    public readonly Vector2 BG => new(B, G);
    public readonly Vector2 BB => new(B, B);
    public readonly Vector2 BA => new(B, A);
    public readonly Vector2 AR => new(A, R);
    public readonly Vector2 AG => new(A, G);
    public readonly Vector2 AB => new(A, B);
    public readonly Vector2 AA => new(A, A);
    public readonly Color RRRR => new(R, R, R, R);
    public readonly Color RRRG => new(R, R, R, G);
    public readonly Color RRRB => new(R, R, R, B);
    public readonly Color RRRA => new(R, R, R, A);
    public readonly Color RRGR => new(R, R, G, R);
    public readonly Color RRGG => new(R, R, G, G);
    public readonly Color RRGB => new(R, R, G, B);
    public readonly Color RRGA => new(R, R, G, A);
    public readonly Color RRBR => new(R, R, B, R);
    public readonly Color RRBG => new(R, R, B, G);
    public readonly Color RRBB => new(R, R, B, B);
    public readonly Color RRBA => new(R, R, B, A);
    public readonly Color RRAR => new(R, R, A, R);
    public readonly Color RRAG => new(R, R, A, G);
    public readonly Color RRAB => new(R, R, A, B);
    public readonly Color RRAA => new(R, R, A, A);
    public readonly Color RGRR => new(R, G, R, R);
    public readonly Color RGRG => new(R, G, R, G);
    public readonly Color RGRB => new(R, G, R, B);
    public readonly Color RGRA => new(R, G, R, A);
    public readonly Color RGGR => new(R, G, G, R);
    public readonly Color RGGG => new(R, G, G, G);
    public readonly Color RGGB => new(R, G, G, B);
    public readonly Color RGGA => new(R, G, G, A);
    public readonly Color RGBR => new(R, G, B, R);
    public readonly Color RGBG => new(R, G, B, G);
    public readonly Color RGBB => new(R, G, B, B);
    public readonly Color RGBA => new(R, G, B, A);
    public readonly Color RGAR => new(R, G, A, R);
    public readonly Color RGAG => new(R, G, A, G);
    public readonly Color RGAB => new(R, G, A, B);
    public readonly Color RGAA => new(R, G, A, A);
    public readonly Color RBRR => new(R, B, R, R);
    public readonly Color RBRG => new(R, B, R, G);
    public readonly Color RBRB => new(R, B, R, B);
    public readonly Color RBRA => new(R, B, R, A);
    public readonly Color RBGR => new(R, B, G, R);
    public readonly Color RBGG => new(R, B, G, G);
    public readonly Color RBGB => new(R, B, G, B);
    public readonly Color RBGA => new(R, B, G, A);
    public readonly Color RBBR => new(R, B, B, R);
    public readonly Color RBBG => new(R, B, B, G);
    public readonly Color RBBB => new(R, B, B, B);
    public readonly Color RBBA => new(R, B, B, A);
    public readonly Color RBAR => new(R, B, A, R);
    public readonly Color RBAG => new(R, B, A, G);
    public readonly Color RBAB => new(R, B, A, B);
    public readonly Color RBAA => new(R, B, A, A);
    public readonly Color RARR => new(R, A, R, R);
    public readonly Color RARG => new(R, A, R, G);
    public readonly Color RARB => new(R, A, R, B);
    public readonly Color RARA => new(R, A, R, A);
    public readonly Color RAGR => new(R, A, G, R);
    public readonly Color RAGG => new(R, A, G, G);
    public readonly Color RAGB => new(R, A, G, B);
    public readonly Color RAGA => new(R, A, G, A);
    public readonly Color RABR => new(R, A, B, R);
    public readonly Color RABG => new(R, A, B, G);
    public readonly Color RABB => new(R, A, B, B);
    public readonly Color RABA => new(R, A, B, A);
    public readonly Color RAAR => new(R, A, A, R);
    public readonly Color RAAG => new(R, A, A, G);
    public readonly Color RAAB => new(R, A, A, B);
    public readonly Color RAAA => new(R, A, A, A);
    public readonly Color GRRR => new(G, R, R, R);
    public readonly Color GRRG => new(G, R, R, G);
    public readonly Color GRRB => new(G, R, R, B);
    public readonly Color GRRA => new(G, R, R, A);
    public readonly Color GRGR => new(G, R, G, R);
    public readonly Color GRGG => new(G, R, G, G);
    public readonly Color GRGB => new(G, R, G, B);
    public readonly Color GRGA => new(G, R, G, A);
    public readonly Color GRBR => new(G, R, B, R);
    public readonly Color GRBG => new(G, R, B, G);
    public readonly Color GRBB => new(G, R, B, B);
    public readonly Color GRBA => new(G, R, B, A);
    public readonly Color GRAR => new(G, R, A, R);
    public readonly Color GRAG => new(G, R, A, G);
    public readonly Color GRAB => new(G, R, A, B);
    public readonly Color GRAA => new(G, R, A, A);
    public readonly Color GGRR => new(G, G, R, R);
    public readonly Color GGRG => new(G, G, R, G);
    public readonly Color GGRB => new(G, G, R, B);
    public readonly Color GGRA => new(G, G, R, A);
    public readonly Color GGGR => new(G, G, G, R);
    public readonly Color GGGG => new(G, G, G, G);
    public readonly Color GGGB => new(G, G, G, B);
    public readonly Color GGGA => new(G, G, G, A);
    public readonly Color GGBR => new(G, G, B, R);
    public readonly Color GGBG => new(G, G, B, G);
    public readonly Color GGBB => new(G, G, B, B);
    public readonly Color GGBA => new(G, G, B, A);
    public readonly Color GGAR => new(G, G, A, R);
    public readonly Color GGAG => new(G, G, A, G);
    public readonly Color GGAB => new(G, G, A, B);
    public readonly Color GGAA => new(G, G, A, A);
    public readonly Color GBRR => new(G, B, R, R);
    public readonly Color GBRG => new(G, B, R, G);
    public readonly Color GBRB => new(G, B, R, B);
    public readonly Color GBRA => new(G, B, R, A);
    public readonly Color GBGR => new(G, B, G, R);
    public readonly Color GBGG => new(G, B, G, G);
    public readonly Color GBGB => new(G, B, G, B);
    public readonly Color GBGA => new(G, B, G, A);
    public readonly Color GBBR => new(G, B, B, R);
    public readonly Color GBBG => new(G, B, B, G);
    public readonly Color GBBB => new(G, B, B, B);
    public readonly Color GBBA => new(G, B, B, A);
    public readonly Color GBAR => new(G, B, A, R);
    public readonly Color GBAG => new(G, B, A, G);
    public readonly Color GBAB => new(G, B, A, B);
    public readonly Color GBAA => new(G, B, A, A);
    public readonly Color GARR => new(G, A, R, R);
    public readonly Color GARG => new(G, A, R, G);
    public readonly Color GARB => new(G, A, R, B);
    public readonly Color GARA => new(G, A, R, A);
    public readonly Color GAGR => new(G, A, G, R);
    public readonly Color GAGG => new(G, A, G, G);
    public readonly Color GAGB => new(G, A, G, B);
    public readonly Color GAGA => new(G, A, G, A);
    public readonly Color GABR => new(G, A, B, R);
    public readonly Color GABG => new(G, A, B, G);
    public readonly Color GABB => new(G, A, B, B);
    public readonly Color GABA => new(G, A, B, A);
    public readonly Color GAAR => new(G, A, A, R);
    public readonly Color GAAG => new(G, A, A, G);
    public readonly Color GAAB => new(G, A, A, B);
    public readonly Color GAAA => new(G, A, A, A);
    public readonly Color BRRR => new(B, R, R, R);
    public readonly Color BRRG => new(B, R, R, G);
    public readonly Color BRRB => new(B, R, R, B);
    public readonly Color BRRA => new(B, R, R, A);
    public readonly Color BRGR => new(B, R, G, R);
    public readonly Color BRGG => new(B, R, G, G);
    public readonly Color BRGB => new(B, R, G, B);
    public readonly Color BRGA => new(B, R, G, A);
    public readonly Color BRBR => new(B, R, B, R);
    public readonly Color BRBG => new(B, R, B, G);
    public readonly Color BRBB => new(B, R, B, B);
    public readonly Color BRBA => new(B, R, B, A);
    public readonly Color BRAR => new(B, R, A, R);
    public readonly Color BRAG => new(B, R, A, G);
    public readonly Color BRAB => new(B, R, A, B);
    public readonly Color BRAA => new(B, R, A, A);
    public readonly Color BGRR => new(B, G, R, R);
    public readonly Color BGRG => new(B, G, R, G);
    public readonly Color BGRB => new(B, G, R, B);
    public readonly Color BGRA => new(B, G, R, A);
    public readonly Color BGGR => new(B, G, G, R);
    public readonly Color BGGG => new(B, G, G, G);
    public readonly Color BGGB => new(B, G, G, B);
    public readonly Color BGGA => new(B, G, G, A);
    public readonly Color BGBR => new(B, G, B, R);
    public readonly Color BGBG => new(B, G, B, G);
    public readonly Color BGBB => new(B, G, B, B);
    public readonly Color BGBA => new(B, G, B, A);
    public readonly Color BGAR => new(B, G, A, R);
    public readonly Color BGAG => new(B, G, A, G);
    public readonly Color BGAB => new(B, G, A, B);
    public readonly Color BGAA => new(B, G, A, A);
    public readonly Color BBRR => new(B, B, R, R);
    public readonly Color BBRG => new(B, B, R, G);
    public readonly Color BBRB => new(B, B, R, B);
    public readonly Color BBRA => new(B, B, R, A);
    public readonly Color BBGR => new(B, B, G, R);
    public readonly Color BBGG => new(B, B, G, G);
    public readonly Color BBGB => new(B, B, G, B);
    public readonly Color BBGA => new(B, B, G, A);
    public readonly Color BBBR => new(B, B, B, R);
    public readonly Color BBBG => new(B, B, B, G);
    public readonly Color BBBB => new(B, B, B, B);
    public readonly Color BBBA => new(B, B, B, A);
    public readonly Color BBAR => new(B, B, A, R);
    public readonly Color BBAG => new(B, B, A, G);
    public readonly Color BBAB => new(B, B, A, B);
    public readonly Color BBAA => new(B, B, A, A);
    public readonly Color BARR => new(B, A, R, R);
    public readonly Color BARG => new(B, A, R, G);
    public readonly Color BARB => new(B, A, R, B);
    public readonly Color BARA => new(B, A, R, A);
    public readonly Color BAGR => new(B, A, G, R);
    public readonly Color BAGG => new(B, A, G, G);
    public readonly Color BAGB => new(B, A, G, B);
    public readonly Color BAGA => new(B, A, G, A);
    public readonly Color BABR => new(B, A, B, R);
    public readonly Color BABG => new(B, A, B, G);
    public readonly Color BABB => new(B, A, B, B);
    public readonly Color BABA => new(B, A, B, A);
    public readonly Color BAAR => new(B, A, A, R);
    public readonly Color BAAG => new(B, A, A, G);
    public readonly Color BAAB => new(B, A, A, B);
    public readonly Color BAAA => new(B, A, A, A);
    public readonly Color ARRR => new(A, R, R, R);
    public readonly Color ARRG => new(A, R, R, G);
    public readonly Color ARRB => new(A, R, R, B);
    public readonly Color ARRA => new(A, R, R, A);
    public readonly Color ARGR => new(A, R, G, R);
    public readonly Color ARGG => new(A, R, G, G);
    public readonly Color ARGB => new(A, R, G, B);
    public readonly Color ARGA => new(A, R, G, A);
    public readonly Color ARBR => new(A, R, B, R);
    public readonly Color ARBG => new(A, R, B, G);
    public readonly Color ARBB => new(A, R, B, B);
    public readonly Color ARBA => new(A, R, B, A);
    public readonly Color ARAR => new(A, R, A, R);
    public readonly Color ARAG => new(A, R, A, G);
    public readonly Color ARAB => new(A, R, A, B);
    public readonly Color ARAA => new(A, R, A, A);
    public readonly Color AGRR => new(A, G, R, R);
    public readonly Color AGRG => new(A, G, R, G);
    public readonly Color AGRB => new(A, G, R, B);
    public readonly Color AGRA => new(A, G, R, A);
    public readonly Color AGGR => new(A, G, G, R);
    public readonly Color AGGG => new(A, G, G, G);
    public readonly Color AGGB => new(A, G, G, B);
    public readonly Color AGGA => new(A, G, G, A);
    public readonly Color AGBR => new(A, G, B, R);
    public readonly Color AGBG => new(A, G, B, G);
    public readonly Color AGBB => new(A, G, B, B);
    public readonly Color AGBA => new(A, G, B, A);
    public readonly Color AGAR => new(A, G, A, R);
    public readonly Color AGAG => new(A, G, A, G);
    public readonly Color AGAB => new(A, G, A, B);
    public readonly Color AGAA => new(A, G, A, A);
    public readonly Color ABRR => new(A, B, R, R);
    public readonly Color ABRG => new(A, B, R, G);
    public readonly Color ABRB => new(A, B, R, B);
    public readonly Color ABRA => new(A, B, R, A);
    public readonly Color ABGR => new(A, B, G, R);
    public readonly Color ABGG => new(A, B, G, G);
    public readonly Color ABGB => new(A, B, G, B);
    public readonly Color ABGA => new(A, B, G, A);
    public readonly Color ABBR => new(A, B, B, R);
    public readonly Color ABBG => new(A, B, B, G);
    public readonly Color ABBB => new(A, B, B, B);
    public readonly Color ABBA => new(A, B, B, A);
    public readonly Color ABAR => new(A, B, A, R);
    public readonly Color ABAG => new(A, B, A, G);
    public readonly Color ABAB => new(A, B, A, B);
    public readonly Color ABAA => new(A, B, A, A);
    public readonly Color AARR => new(A, A, R, R);
    public readonly Color AARG => new(A, A, R, G);
    public readonly Color AARB => new(A, A, R, B);
    public readonly Color AARA => new(A, A, R, A);
    public readonly Color AAGR => new(A, A, G, R);
    public readonly Color AAGG => new(A, A, G, G);
    public readonly Color AAGB => new(A, A, G, B);
    public readonly Color AAGA => new(A, A, G, A);
    public readonly Color AABR => new(A, A, B, R);
    public readonly Color AABG => new(A, A, B, G);
    public readonly Color AABB => new(A, A, B, B);
    public readonly Color AABA => new(A, A, B, A);
    public readonly Color AAAR => new(A, A, A, R);
    public readonly Color AAAG => new(A, A, A, G);
    public readonly Color AAAB => new(A, A, A, B);
    public readonly Color AAAA => new(A, A, A, A);
}
