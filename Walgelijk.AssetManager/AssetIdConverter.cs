using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

public class AssetIdConverter : JsonConverter<AssetId>
{
    public override AssetId ReadJson(JsonReader reader, Type objectType, AssetId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string s = reader.Value?.ToString() ?? throw new Exception("Invalid AssetId: null value");

        if (int.TryParse(s, out var id))
            return new AssetId(id);

        return new AssetId(s);
    }

    public override void WriteJson(JsonWriter writer, AssetId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
