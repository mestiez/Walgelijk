using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

public class AssetRefConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(AssetRef<>);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var s = new GlobalAssetIdConverter().ReadJson(reader, objectType, default, false, serializer);

        return Activator.CreateInstance(objectType, s);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else
            writer.WriteValue(value.ToString());
    }
}