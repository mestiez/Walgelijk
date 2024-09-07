
using System.Globalization;

namespace Walgelijk.AssetManager;

public static class IdUtil
{
    public static int Hash(ReadOnlySpan<char> a) => Hashes.MurmurHash1(a);

    public static int Convert(ReadOnlySpan<char> a)
    {
        if (TryConvert(a, out var result))
            return result;

        throw new ArgumentException($"Given ID \"{a}\" is invalid");
    }

    public static bool TryConvert(ReadOnlySpan<char> a, out int result)
    {
        return int.TryParse(a, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public static string Convert(int a)
    {
        if (TryConvert(a, out var result))
            return result;

        throw new ArgumentException($"Given ID \"{a}\" is invalid");
    }

    public static bool TryConvert(int a, out string result)
    {
        result = a.ToString("D", CultureInfo.InvariantCulture);
        return true;
    }
}