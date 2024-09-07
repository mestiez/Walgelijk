using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

public class AssetIdConverter : JsonConverter<AssetId>
{
    public override AssetId ReadJson(JsonReader reader, Type objectType, AssetId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var s = reader.Value?.ToString();
        if (string.IsNullOrWhiteSpace(s))
            return AssetId.None;

        if (AssetId.TryParse(s, out var id))
            return id;

        return new AssetId(s);
    }

    public override void WriteJson(JsonWriter writer, AssetId value, JsonSerializer serializer)
    {
        if (value == AssetId.None)
            writer.WriteNull();
        else
            writer.WriteValue(value.ToString());
    }
}
