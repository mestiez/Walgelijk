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

    public static string Get(in string key, string? fallback = null)
    {
        var lang = CurrentLanguage ?? FallbackLanguage;
        if (lang == null || (!lang.Table.TryGetValue(key, out var value) && !(FallbackLanguage?.Table.TryGetValue(key, out value) ?? false)))
            return fallback ?? key;
        return value;
    }
}
