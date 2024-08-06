using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

public class GlobalAssetIdConverter : JsonConverter<GlobalAssetId>
{
    public override GlobalAssetId ReadJson(JsonReader reader, Type objectType, GlobalAssetId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string? s = reader.Value?.ToString();

        if (string.IsNullOrWhiteSpace(s))
            return GlobalAssetId.None;
        var index = s.IndexOf(':');

        if (index != -1)
        {
            var external = s[..index];
            var @internal = s[(index + 1)..];

            if (!PackageId.TryParse(external, out var externalId))
                externalId = new PackageId(external);

            if (!AssetId.TryParse(@internal, out var internalId))
                internalId = new AssetId(@internal);

            return new GlobalAssetId(externalId, internalId);
        }
        else
        {
            // agnostic ID

            if (!AssetId.TryParse(s, out var internalId))
                internalId = new AssetId(s);

            return new GlobalAssetId(internalId);
        }
    }

    public override void WriteJson(JsonWriter writer, GlobalAssetId value, JsonSerializer serializer)
    {
        if (value == GlobalAssetId.None)
            writer.WriteNull();
        else
            writer.WriteValue(value.ToNamedString());
    }
}
