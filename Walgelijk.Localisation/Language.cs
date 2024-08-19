using Newtonsoft.Json;
using System.Globalization;

namespace Walgelijk.Localisation;

public class Language
{
    public readonly string DisplayName;
    public readonly CultureInfo Culture;
    public IReadableTexture Flag = Flags.Unknown;

    public Dictionary<string, string> Table = new();

    public Language(string displayName, CultureInfo culture, IReadableTexture? flag = null)
    {
        DisplayName = displayName;
        Culture = culture;
        Flag = flag ?? Flags.Unknown;
    }

    public Language(string displayName, CultureInfo culture, IReadableTexture flag, Dictionary<string, string> table)
    {
        DisplayName = displayName;
        Culture = culture;
        Flag = flag;
        Table = table;
    }

    public Language(string displayName, CultureInfo culture, Dictionary<string, string> table)
    {
        DisplayName = displayName;
        Culture = culture;
        Flag = Flags.Unknown;
        Table = table;
    }

    public override string ToString() => DisplayName;

    public static Language Load(string filePath)
    {
        var data = File.ReadAllText(filePath);
        var s = JsonConvert.DeserializeObject<Serialisable>(data);
        return new Language(
            s.DisplayName ?? Path.GetFileNameWithoutExtension(filePath),
            string.IsNullOrWhiteSpace(s.Culture) ? CultureInfo.InvariantCulture : new CultureInfo(s.Culture),
            string.IsNullOrWhiteSpace(s.Flag) ? Flags.Unknown : Resources.Load<Texture>(Path.GetFullPath(s.Flag, Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory), true),
            s.Table ?? new Dictionary<string, string>());
    }

    private struct Serialisable
    {
        public string? DisplayName;
        public string? Culture;
        public string? Flag;
        public Dictionary<string, string>? Table;
    }
}
