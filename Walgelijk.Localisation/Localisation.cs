using System.Globalization;

namespace Walgelijk.Localisation;

public static class Localisation
{
    /// <summary>
    /// Currently selected language
    /// </summary>
    public static Language? CurrentLanguage;

    /// <summary>
    /// Language to display
    /// </summary>
    public static Language? FallbackLanguage;
    public static readonly HashSet<Language> Languages = new();

    public static string Get(in string key, string? fallback = null)
    {
        var lang = CurrentLanguage ?? FallbackLanguage;
        if (lang == null || !lang.Table.TryGetValue(key, out var value))
            return fallback ?? key;
        return value;
    }
}

public class Language
{
    public readonly string DisplayName;
    public readonly CultureInfo Culture;
    public IReadableTexture Flag = Flags.Unknown;

    public readonly Dictionary<string, string> Table = new();

    public Language(string displayName, CultureInfo culture, IReadableTexture flag)
    {
        DisplayName = displayName;
        Culture = culture;
        Flag = flag;
    }

    public override string ToString() => DisplayName;
}

public readonly struct Flags
{
    public static readonly Texture Unknown = TextureLoader.FromBytes(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 64, 0, 0, 0, 48, 8, 3, 0, 0, 0, 150, 149, 140, 45, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 4, 103, 65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 189, 80, 76, 84, 69, 40, 40, 40, 46, 46, 46, 110, 110, 110, 141, 141, 141, 164, 164, 164, 147, 147, 147, 124, 124, 124, 66, 66, 66, 55, 55, 55, 162, 162, 162, 252, 252, 252, 255, 255, 255, 190, 190, 190, 73, 73, 73, 63, 63, 63, 223, 223, 223, 104, 104, 104, 205, 205, 205, 217, 217, 217, 64, 64, 64, 93, 93, 93, 145, 145, 145, 132, 132, 132, 144, 144, 144, 152, 152, 152, 208, 208, 208, 192, 192, 192, 215, 215, 215, 133, 133, 133, 115, 115, 115, 138, 138, 138, 201, 201, 201, 47, 47, 47, 168, 168, 168, 67, 67, 67, 209, 209, 209, 82, 82, 82, 228, 228, 228, 126, 126, 126, 146, 146, 146, 232, 232, 232, 222, 222, 222, 96, 96, 96, 225, 225, 225, 196, 196, 196, 112, 112, 112, 229, 229, 229, 249, 249, 249, 242, 242, 242, 233, 233, 233, 79, 79, 79, 77, 77, 77, 166, 166, 166, 172, 172, 172, 88, 88, 88, 74, 74, 74, 251, 251, 251, 254, 254, 254, 136, 136, 136, 111, 111, 111, 116, 116, 116, 187, 187, 187, 86, 86, 86, 248, 44, 95, 20, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 14, 195, 0, 0, 14, 195, 1, 199, 111, 168, 100, 0, 0, 0, 207, 73, 68, 65, 84, 72, 75, 237, 212, 199, 14, 194, 48, 16, 4, 208, 64, 232, 75, 13, 161, 247, 222, 123, 175, 255, 255, 89, 172, 205, 28, 56, 226, 189, 32, 33, 191, 67, 52, 99, 105, 124, 73, 20, 199, 178, 172, 47, 4, 130, 110, 40, 28, 137, 162, 25, 139, 197, 19, 164, 37, 83, 56, 49, 147, 206, 188, 231, 74, 22, 103, 70, 114, 122, 234, 233, 39, 229, 113, 104, 192, 87, 187, 2, 135, 162, 10, 165, 247, 161, 137, 50, 207, 50, 58, 85, 56, 85, 117, 50, 82, 227, 89, 93, 167, 6, 39, 79, 39, 161, 38, 95, 208, 66, 150, 104, 171, 183, 209, 65, 17, 232, 246, 120, 79, 125, 52, 115, 254, 64, 237, 195, 104, 230, 134, 106, 78, 35, 52, 129, 49, 207, 39, 83, 20, 1, 151, 247, 179, 24, 138, 196, 156, 47, 88, 32, 139, 44, 137, 86, 136, 50, 107, 162, 13, 162, 204, 150, 33, 254, 200, 110, 127, 56, 34, 138, 156, 206, 68, 151, 29, 138, 196, 85, 125, 134, 46, 138, 196, 77, 93, 112, 71, 17, 121, 136, 254, 69, 159, 182, 79, 4, 203, 250, 107, 142, 243, 2, 100, 249, 12, 236, 142, 134, 196, 250, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 });
}