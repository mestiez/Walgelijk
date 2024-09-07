using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

public class PackageIdConverter : JsonConverter<PackageId>
{
    public override PackageId ReadJson(JsonReader reader, Type objectType, PackageId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var s = reader.Value?.ToString();
        if (string.IsNullOrWhiteSpace(s))
            return PackageId.None;

        if (PackageId.TryParse(s, out var id))
            return id;

        return new PackageId(s);
    }

    public override void WriteJson(JsonWriter writer, PackageId value, JsonSerializer serializer)
    {
        if (value == PackageId.None)
            writer.WriteNull();
        else
            writer.WriteValue(value.ToString());
    }
}
