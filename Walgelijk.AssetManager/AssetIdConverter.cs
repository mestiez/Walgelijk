using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

public class AssetIdConverter : JsonConverter<AssetId>
{
    public override AssetId ReadJson(JsonReader reader, Type objectType, AssetId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string s = reader.Value?.ToString() ?? throw new Exception("Invalid AssetID. Only integers are accepted.");

        return new AssetId(int.Parse(s));
    }

    public override void WriteJson(JsonWriter writer, AssetId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Internal.ToString());
    }
}